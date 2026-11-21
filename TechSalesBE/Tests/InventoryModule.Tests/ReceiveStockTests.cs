using CommonModule.Domain.Exceptions;
using InventoryModule.Application.Services;
using InventoryModule.Application.UseCases.ReceiveStock;
using InventoryModule.Domain.Entities;
using InventoryModule.Domain.Enums;
using InventoryModule.Domain.Exceptions;
using Xunit;

namespace InventoryModule.Tests;

public sealed class ReceiveStockTests
{
    private readonly Guid _productId = Guid.NewGuid();
    private readonly InventoryStoreFake _store = new();
    private readonly ReceiveStock_UCInput_Validator _validator = new();

    [Fact]
    public async Task InitializesStockAndAddsOneInStockUnitPerSerial()
    {
        ReceiveStock_UCOutput result = await Receive("000042", "SN-2");

        Assert.Equal(_productId, result.ProductId);
        Assert.Equal(2, result.ReceivedQuantity);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(nameof(ProductUnitStatus.IN_STOCK), result.Status);
        Assert.Equal(2, _store.Stocks[_productId].Quantity);
        Assert.Equal(0, _store.Stocks[_productId].ReservedQuantity);
        Assert.Equal(1, _store.TransactionCount);
        Assert.Equal(_productId, Assert.Single(_store.LockedProducts));
        Assert.Equal(2, _store.Units.Count);
        Assert.Equal(ProductUnitStatus.IN_STOCK, _store.Units[(_productId, "000042")].Status);
        Assert.Equal(ProductUnitStatus.IN_STOCK, _store.Units[(_productId, "SN-2")].Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(3)]
    public async Task AddsToExistingQuantityAndPreservesReservations(int? reservedQuantity)
    {
        _store.Stocks.Add(_productId, new Inventory(_productId, 10, reservedQuantity));

        ReceiveStock_UCOutput result = await Receive("SN-1", "SN-2");

        Assert.Equal(12, result.Quantity);
        Assert.Equal(12, _store.Stocks[_productId].Quantity);
        Assert.Equal(reservedQuantity, _store.Stocks[_productId].ReservedQuantity);
    }

    [Fact]
    public async Task RejectsQuantityOverflowBeforeWritingUnits()
    {
        _store.Stocks.Add(_productId, new Inventory(_productId, int.MaxValue, 3));

        DomainException error = await Assert.ThrowsAsync<DomainException>(() => Receive("SN-1"));

        Assert.Equal(DomainExceptionCode.STOCK_QUANTITY_LIMIT_EXCEEDED, error.Code);
        Assert.Equal(int.MaxValue, _store.Stocks[_productId].Quantity);
        Assert.Empty(_store.Units);
    }

    [Fact]
    public void RejectsAnEmptyProductIdAndAnEmptySerialList()
    {
        var result = _validator.Validate(new ReceiveStock_UCInput(Guid.Empty, []));

        Assert.Contains(result.Errors, error => error.PropertyName == "ProductId");
        Assert.Contains(result.Errors, error => error.PropertyName == "SerialNumbers");
    }

    [Fact]
    public void RejectsDuplicateSerialsInTheSameRequest()
    {
        var result = _validator.Validate(new ReceiveStock_UCInput(_productId, ["SN-1", "SN-1"]));

        Assert.Equal("SerialNumbers", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("SN\0-1")]
    public void RejectsInvalidSerialsAtTheirInputIndex(string serialNumber)
    {
        var result = _validator.Validate(new ReceiveStock_UCInput(_productId, ["SN-1", serialNumber]));

        Assert.Equal("SerialNumbers[1]", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void EnforcesTheSerialLengthLimitInUnicodeCharacters(int length, bool valid)
    {
        string serialNumber = string.Concat(Enumerable.Repeat(char.ConvertFromUtf32(0x1F600), length));
        var result = _validator.Validate(new ReceiveStock_UCInput(_productId, [serialNumber]));

        Assert.Equal(valid, result.IsValid);
    }

    private Task<ReceiveStock_UCOutput> Receive(params string[] serialNumbers)
    {
        ReceiveStock_UCHandler handler = new ReceiveStock_UCHandler(new StockReceiver(_store), _store);
        return handler.Handle(new ReceiveStock_UCInput(_productId, serialNumbers.ToList()), CancellationToken.None);
    }
}
