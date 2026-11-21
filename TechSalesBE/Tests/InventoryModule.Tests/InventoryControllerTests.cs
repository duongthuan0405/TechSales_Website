using CommonModule.Presentation.ApiResponseModels;
using InventoryModule.Application.UseCases.ImportStock;
using InventoryModule.Application.UseCases.ReceiveStock;
using InventoryModule.Application.UseCases.UpdateProductUnitStatus;
using InventoryModule.Presentation.Controllers;
using InventoryModule.Presentation.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InventoryModule.Tests;

public sealed class InventoryControllerTests
{
    private readonly Guid _productId = Guid.NewGuid();

    [Fact]
    public async Task ReceiveStockMapsTheRouteAndJsonBodyToTheUseCase()
    {
        ReceiveStock_UCOutput output = new ReceiveStock_UCOutput(_productId, 2, 12, "IN_STOCK");
        SenderStub sender = new SenderStub(request =>
        {
            ReceiveStock_UCInput input = Assert.IsType<ReceiveStock_UCInput>(request);
            Assert.Equal(_productId, input.ProductId);
            Assert.Equal<string>(["SN-1", "SN-2"], input.SerialNumbers);
            return output;
        });
        InventoryController controller = new InventoryController(sender);

        IActionResult actionResult = await controller.ReceiveStock(
            _productId, new ReceiveStockRequest(["SN-1", "SN-2"]), CancellationToken.None);

        AssertSuccess(actionResult, output, "Stock received.");
    }

    [Fact]
    public async Task UpdateStatusMapsTheProductSerialAndStatusToTheUseCase()
    {
        UpdateProductUnitStatus_UCOutput output = new UpdateProductUnitStatus_UCOutput(_productId, "SN-1", "DAMAGED", 2);
        SenderStub sender = new SenderStub(request =>
        {
            UpdateProductUnitStatus_UCInput input = Assert.IsType<UpdateProductUnitStatus_UCInput>(request);
            Assert.Equal(new UpdateProductUnitStatus_UCInput(_productId, "SN-1", "damaged"), input);
            return output;
        });
        InventoryController controller = new InventoryController(sender);

        IActionResult actionResult = await controller.UpdateProductUnitStatus(
            _productId, new UpdateProductUnitStatusRequest("SN-1", "damaged"), CancellationToken.None);

        AssertSuccess(actionResult, output, "Product unit status updated.");
    }

    [Fact]
    public async Task ImportStockPassesTheOpenUploadStreamAndFileMetadataToTheUseCase()
    {
        using MemoryStream fileContent = new MemoryStream([1, 2, 3]);
        FormFile file = new FormFile(fileContent, 0, fileContent.Length, "file", "stock.xlsx");
        ImportStock_UCOutput output = new ImportStock_UCOutput(
            1, "IN_STOCK", [new ImportStock_UCOutput.ProductStock(_productId, 1, 1)]);
        SenderStub sender = new SenderStub(request =>
        {
            ImportStock_UCInput input = Assert.IsType<ImportStock_UCInput>(request);
            Assert.Equal("stock.xlsx", input.FileName);
            Assert.Equal(3, input.Length);
            Assert.Equal(1, input.Content.ReadByte());
            return output;
        });
        InventoryController controller = new InventoryController(sender);

        IActionResult actionResult = await controller.ImportStock(new ImportStockRequest(file), CancellationToken.None);

        AssertSuccess(actionResult, output, "Stock imported.");
    }

    private static void AssertSuccess<T>(IActionResult result, T expectedData, string expectedMessage)
    {
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        ApiSuccessResponse<T> response = Assert.IsType<ApiSuccessResponse<T>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(expectedMessage, response.Message);
        Assert.Equal(expectedData, response.Data);
    }

    private sealed class SenderStub : ISender
    {
        private readonly Func<object, object> _send;

        public SenderStub(Func<object, object> send)
        {
            _send = send;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            Task.FromResult((TResponse)_send(request));

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
            throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
