using CommonModule.BusinessExceptions;
using InventoryModule.Application.Constants;
using InventoryModule.Application.Exceptions;
using InventoryModule.Application.Services;
using InventoryModule.Application.UseCases.ImportStock;
using InventoryModule.Domain.Entities;
using InventoryModule.Domain.Enums;
using Xunit;

namespace InventoryModule.Tests;

public sealed class ImportStockTests
{
    private static readonly Guid FirstProduct = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid SecondProduct = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private readonly InventoryStoreFake _store = new();

    [Fact]
    public async Task ImportsAllProductGroupsInOneTransaction()
    {
        _store.Stocks.Add(FirstProduct, new Inventory(FirstProduct, 10, 3));

        ImportStock_UCOutput result = await Import(
            new StockImportRow(2, SecondProduct.ToString(), "000042"),
            new StockImportRow(3, FirstProduct.ToString(), "000042"),
            new StockImportRow(4, FirstProduct.ToString(), "SN-2"));

        Assert.Equal(3, result.ReceivedQuantity);
        Assert.Equal(nameof(ProductUnitStatus.IN_STOCK), result.Status);
        Assert.Collection(result.Products,
            product => Assert.Equal(new ImportStock_UCOutput.ProductStock(FirstProduct, 2, 12), product),
            product => Assert.Equal(new ImportStock_UCOutput.ProductStock(SecondProduct, 1, 1), product));
        Assert.Equal(1, _store.TransactionCount);
        Assert.Equal<Guid>([FirstProduct, SecondProduct], _store.LockedProducts);
        Assert.Equal(12, _store.Stocks[FirstProduct].Quantity);
        Assert.Equal(3, _store.Stocks[FirstProduct].ReservedQuantity);
        Assert.Equal(1, _store.Stocks[SecondProduct].Quantity);
        Assert.Equal(3, _store.Units.Count);
        Assert.Equal(ProductUnitStatus.IN_STOCK, _store.Units[(FirstProduct, "000042")].Status);
        Assert.Equal(ProductUnitStatus.IN_STOCK, _store.Units[(SecondProduct, "000042")].Status);
    }

    [Fact]
    public async Task ReportsInvalidProductIdsAndSerialsByExcelRowBeforeWriting()
    {
        BusinessException error = await Assert.ThrowsAsync<BusinessException>(() => Import(
            new StockImportRow(2, FirstProduct.ToString(), "SN-VALID"),
            new StockImportRow(3, "not-a-guid", "SN-1"),
            new StockImportRow(4, FirstProduct.ToString(), " "),
            new StockImportRow(6, Guid.Empty.ToString(), new string('A', 101))));

        Assert.Equal(BusinessExceptionCode.BAD_REQUEST, error.Code);
        Assert.Equal(4, error.Errors.Count);
        Assert.Equal(BusinessExceptionMessage.InvalidProductId, Assert.Single(error.Errors["Rows[3].ProductId"]));
        Assert.Equal(BusinessExceptionMessage.InvalidSerialNumber, Assert.Single(error.Errors["Rows[4].SerialNumber"]));
        Assert.Contains("Rows[6].ProductId", error.Errors.Keys);
        Assert.Contains("Rows[6].SerialNumber", error.Errors.Keys);
        Assert.Equal(0, _store.TransactionCount);
        Assert.Empty(_store.Stocks);
        Assert.Empty(_store.Units);
    }

    [Fact]
    public async Task DetectsDuplicateProductSerialPairsEvenWithDifferentGuidFormats()
    {
        BusinessException error = await Assert.ThrowsAsync<BusinessException>(() => Import(
            new StockImportRow(2, FirstProduct.ToString("D"), "SN-1"),
            new StockImportRow(5, FirstProduct.ToString("B"), "SN-1")));

        Assert.Equal(BusinessExceptionCode.BAD_REQUEST, error.Code);
        Assert.Equal(BusinessExceptionMessage.DuplicateImportUnit, Assert.Single(error.Errors["Rows[5].SerialNumber"]));
        Assert.Equal(0, _store.TransactionCount);
        Assert.Empty(_store.Units);
    }

    [Theory]
    [InlineData("inventory.xlsx", 0, "Upload a non-empty Excel workbook.")]
    [InlineData("inventory.xls", 100, "The import file must have the .xlsx extension.")]
    [InlineData("inventory.xlsx", StockImportLimits.MaxFileBytes + 1, "The import file must not exceed 5 MiB.")]
    public void RejectsInvalidFileMetadata(string fileName, long length, string expectedMessage)
    {
        ImportStock_UCInput_Validator validator = new ImportStock_UCInput_Validator();
        var result = validator.Validate(new ImportStock_UCInput(Stream.Null, fileName, length));

        var error = Assert.Single(result.Errors);
        Assert.Equal("File", error.PropertyName);
        Assert.Equal(expectedMessage, error.ErrorMessage);
    }

    [Fact]
    public void AcceptsTheFileSizeLimitAndAnUppercaseExtension()
    {
        ImportStock_UCInput_Validator validator = new ImportStock_UCInput_Validator();

        Assert.True(validator.Validate(new ImportStock_UCInput(Stream.Null, "stock.XLSX", StockImportLimits.MaxFileBytes)).IsValid);
    }

    private async Task<ImportStock_UCOutput> Import(params StockImportRow[] rows)
    {
        ImportStock_UCHandler handler = new ImportStock_UCHandler(
            new WorkbookReaderStub(rows.ToList()), new StockReceiver(_store), _store);
        return await handler.Handle(new ImportStock_UCInput(Stream.Null, "inventory.xlsx", 100), CancellationToken.None);
    }

    private sealed class WorkbookReaderStub : IStockImportWorkbookReader
    {
        private readonly List<StockImportRow> _rows;

        public WorkbookReaderStub(List<StockImportRow> rows)
        {
            _rows = rows;
        }

        public List<StockImportRow> Read(Stream content, CancellationToken cancellationToken)
        {
            return _rows;
        }
    }
}
