using CommonModule.BusinessExceptions;
using CommonModule.Domain.Exceptions;
using InventoryModule.Application.Exceptions;
using InventoryModule.Application.UseCases.UpdateProductUnitStatus;
using InventoryModule.Domain.Entities;
using InventoryModule.Domain.Enums;
using InventoryModule.Domain.Exceptions;
using Xunit;

namespace InventoryModule.Tests;

public sealed class UpdateProductUnitStatusTests
{
    private const string SerialNumber = "SN-1";
    private readonly Guid _productId = Guid.NewGuid();
    private readonly InventoryStoreFake _store = new();
    private readonly UpdateProductUnitStatus_UCInput_Validator _validator = new();

    [Theory]
    [InlineData("DAMAGED")]
    [InlineData("damaged")]
    public async Task MarksOneInStockUnitDamagedAndReducesAvailableQuantity(string status)
    {
        SeedUnit(ProductUnitStatus.IN_STOCK, 3);
        Assert.True(_validator.Validate(new UpdateProductUnitStatus_UCInput(_productId, SerialNumber, status)).IsValid);

        UpdateProductUnitStatus_UCOutput result = await MarkDamaged(status);

        Assert.Equal(_productId, result.ProductId);
        Assert.Equal(SerialNumber, result.SerialNumber);
        Assert.Equal(nameof(ProductUnitStatus.DAMAGED), result.Status);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(2, _store.Stocks[_productId].Quantity);
        Assert.Equal(2, _store.Stocks[_productId].ReservedQuantity);
        Assert.Equal(ProductUnitStatus.DAMAGED, _store.Units[(_productId, SerialNumber)].Status);
        Assert.Equal(1, _store.TransactionCount);
        Assert.Equal(_productId, Assert.Single(_store.LockedProducts));
    }

    [Theory]
    [InlineData("RESERVED")]
    [InlineData("SOLD")]
    [InlineData(" DAMAGED ")]
    [InlineData("")]
    public void RejectsUnsupportedRequestedStatuses(string status)
    {
        var result = _validator.Validate(new UpdateProductUnitStatus_UCInput(_productId, SerialNumber, status));

        Assert.Equal("Status", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void RejectsAnEmptyProductIdAndAnEmptySerial()
    {
        var result = _validator.Validate(new UpdateProductUnitStatus_UCInput(Guid.Empty, "", "DAMAGED"));

        Assert.Contains(result.Errors, error => error.PropertyName == "ProductId");
        Assert.Contains(result.Errors, error => error.PropertyName == "SerialNumber");
    }

    [Fact]
    public async Task ReportsMissingStockWithoutCreatingARecord()
    {
        BusinessException error = await Assert.ThrowsAsync<BusinessException>(() => MarkDamaged());

        Assert.Equal(BusinessExceptionCode.NOT_FOUND, error.Code);
        Assert.Equal(BusinessExceptionMessage.StockNotFound, error.Message);
        Assert.Empty(_store.Stocks);
    }

    [Fact]
    public async Task DoesNotFindTheSameSerialOnAnotherProduct()
    {
        Guid otherProductId = Guid.NewGuid();
        _store.Stocks.Add(_productId, new Inventory(_productId, 3, 0));
        _store.Units.Add((otherProductId, SerialNumber), new ProductUnit(otherProductId, SerialNumber));

        BusinessException error = await Assert.ThrowsAsync<BusinessException>(() => MarkDamaged());

        Assert.Equal(BusinessExceptionCode.NOT_FOUND, error.Code);
        Assert.Equal(BusinessExceptionMessage.ProductUnitNotFound, error.Message);
        Assert.Equal(3, _store.Stocks[_productId].Quantity);
        Assert.Equal(ProductUnitStatus.IN_STOCK, _store.Units[(otherProductId, SerialNumber)].Status);
    }

    [Theory]
    [InlineData("RESERVED")]
    [InlineData("SOLD")]
    [InlineData("DAMAGED")]
    public async Task RejectsAUnitThatIsNotInStock(string currentStatus)
    {
        ProductUnitStatus status = Enum.Parse<ProductUnitStatus>(currentStatus);
        SeedUnit(status, 3);

        DomainException error = await Assert.ThrowsAsync<DomainException>(() => MarkDamaged());

        Assert.Equal(DomainExceptionCode.INVALID_PRODUCT_UNIT_TRANSITION, error.Code);
        Assert.Equal(3, _store.Stocks[_productId].Quantity);
        Assert.Equal(status, _store.Units[(_productId, SerialNumber)].Status);
    }

    [Fact]
    public async Task DoesNotPersistDamageWhenAvailableStockIsZero()
    {
        SeedUnit(ProductUnitStatus.IN_STOCK, 0);

        DomainException error = await Assert.ThrowsAsync<DomainException>(() => MarkDamaged());

        Assert.Equal(DomainExceptionCode.INSUFFICIENT_AVAILABLE_STOCK, error.Code);
        Assert.Equal(0, _store.Stocks[_productId].Quantity);
        Assert.Equal(ProductUnitStatus.IN_STOCK, _store.Units[(_productId, SerialNumber)].Status);
    }

    [Theory]
    [InlineData("IN_STOCK")]
    [InlineData("in_stock")]
    public async Task RestoresDamagedUnitAndOnlyIncrementsAvailableStockOnce(string target)
    {
        SeedUnit(ProductUnitStatus.DAMAGED, 0);
        Assert.True(_validator.Validate(new UpdateProductUnitStatus_UCInput(_productId, SerialNumber, target)).IsValid);
        var result = await MarkDamaged(target);
        Assert.Equal("IN_STOCK", result.Status);
        Assert.Equal(1, result.Quantity);
        Assert.Equal(2, _store.Stocks[_productId].ReservedQuantity);
        await Assert.ThrowsAsync<DomainException>(() => MarkDamaged(target));
        Assert.Equal(1, _store.Stocks[_productId].Quantity);
        Assert.Equal(ProductUnitStatus.IN_STOCK, _store.Units[(_productId, SerialNumber)].Status);
    }

    [Theory]
    [InlineData("RESERVED")]
    [InlineData("SOLD")]
    public async Task RestorationCannotReleaseReservedOrSoldUnits(string currentStatus)
    {
        ProductUnitStatus status = Enum.Parse<ProductUnitStatus>(currentStatus);
        SeedUnit(status, 3);
        await Assert.ThrowsAsync<DomainException>(() => MarkDamaged("IN_STOCK"));
        Assert.Equal(3, _store.Stocks[_productId].Quantity);
        Assert.Equal(status, _store.Units[(_productId, SerialNumber)].Status);
    }

    [Fact]
    public async Task RestorationRejectsOverflowWithoutPersistingUnitChange()
    {
        SeedUnit(ProductUnitStatus.DAMAGED, int.MaxValue);
        var error = await Assert.ThrowsAsync<DomainException>(() => MarkDamaged("IN_STOCK"));
        Assert.Equal(DomainExceptionCode.STOCK_QUANTITY_LIMIT_EXCEEDED, error.Code);
        Assert.Equal(ProductUnitStatus.DAMAGED, _store.Units[(_productId, SerialNumber)].Status);
        Assert.Equal(int.MaxValue, _store.Stocks[_productId].Quantity);
    }

    private void SeedUnit(ProductUnitStatus status, int quantity)
    {
        _store.Stocks.Add(_productId, new Inventory(_productId, quantity, 2));
        _store.Units.Add((_productId, SerialNumber), new ProductUnit(_productId, SerialNumber, status));
    }

    private Task<UpdateProductUnitStatus_UCOutput> MarkDamaged(string status = "DAMAGED")
    {
        UpdateProductUnitStatus_UCHandler handler = new UpdateProductUnitStatus_UCHandler(_store, _store);
        return handler.Handle(new UpdateProductUnitStatus_UCInput(_productId, SerialNumber, status), CancellationToken.None);
    }
}
