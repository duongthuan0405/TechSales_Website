using CatalogModule.Contracts;
using CatalogModule.Application.Ports;
using CartModule.Application.Ports;
using InventoryModule.Application.Ports;
using InventoryModule.Contracts;
using Microsoft.Extensions.DependencyInjection;
using TechSalesBE.WebAPI.Integration;
using Xunit;

namespace ModuleContracts.Tests;

public sealed class ModuleContractTests
{
    [Fact]
    public async Task Host_adapters_resolve_scoped_contracts_and_map_batched_product_and_stock_data()
    {
        ServiceCollection services = new ServiceCollection();
        CatalogFake catalog = new CatalogFake();
        Guid id = Guid.NewGuid();
        catalog.Products.Add(new CatalogProduct(id, "Laptop", "Description", 15000000, "ACTIVE",
            Guid.NewGuid(), "Brand", Guid.NewGuid(), "Category",
            new List<CatalogProductImage> { new CatalogProductImage(Guid.NewGuid(), "/product-images/photo.png", true) }));
        services.AddSingleton<IProductCatalog>(catalog);
        services.AddScoped<IInventoryQuery, StockFake>();
        IntegrationRegistration.RegisterModule(services);
        await using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        List<Guid> ids = new List<Guid> { id, Guid.NewGuid() };

        List<CartProduct> products = await scope.ServiceProvider.GetRequiredService<ICartProducts>().GetProductsAsync(ids, default);
        CartProduct product = Assert.Single(products);
        Assert.Equal(id, product.Id);
        Assert.Equal(15000000, product.Price);
        Assert.Equal("Brand", product.BrandName);
        Assert.True(Assert.Single(product.Images).IsPrimary);
        List<Guid> existing = await scope.ServiceProvider.GetRequiredService<IInventoryProducts>().GetExistingProductIdsAsync(ids, default);
        Assert.Equal(new List<Guid> { id }, existing);
        List<ProductInventory> stocks = await scope.ServiceProvider.GetRequiredService<IProductInventory>().GetStocksAsync(ids, default);
        Assert.Equal(5, stocks.Single(stock => stock.ProductId == id).Quantity);
        Assert.Equal(2, stocks.Single(stock => stock.ProductId == id).ReservedQuantity);
    }

    private sealed class CatalogFake : IProductCatalog
    {
        public List<CatalogProduct> Products { get; } = new List<CatalogProduct>();
        public Task<List<CatalogProduct>> GetProductsAsync(List<Guid> ids, CancellationToken ct)
        {
            return Task.FromResult(Products.Where(product => ids.Contains(product.Id)).ToList());
        }
    }

    private sealed class StockFake : IInventoryQuery
    {
        public Task<List<ProductStock>> GetStocksAsync(List<Guid> ids, CancellationToken ct)
        {
            return Task.FromResult(ids.Select(id => new ProductStock(id, 5, 2)).ToList());
        }
    }
}
