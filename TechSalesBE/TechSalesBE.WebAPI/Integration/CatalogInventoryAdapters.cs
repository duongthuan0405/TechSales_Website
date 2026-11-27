using CatalogModule.Application.Ports;
using CatalogModule.Contracts;
using CartModule.Application.Ports;
using InventoryModule.Application.Ports;
using InventoryModule.Contracts;

namespace TechSalesBE.WebAPI.Integration;

internal sealed class CartProductsAdapter : ICartProducts
{
    private readonly IProductCatalog _catalog;

    public CartProductsAdapter(IProductCatalog catalog)
    {
        _catalog = catalog;
    }

    public async Task<List<CartProduct>> GetProductsAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        List<CatalogProduct> products = await _catalog.GetProductsAsync(productIds, cancellationToken);
        return products.Select(product => new CartProduct(product.Id, product.Name, product.Description, product.Price,
            product.Status, product.BrandId, product.BrandName, product.CategoryId, product.CategoryName,
            product.Images.Select(image => new CartProductImage(image.Id, image.ImageUrl, image.IsPrimary)).ToList())).ToList();
    }
}

internal sealed class InventoryProductsAdapter : IInventoryProducts
{
    private readonly IProductCatalog _catalog;

    public InventoryProductsAdapter(IProductCatalog catalog)
    {
        _catalog = catalog;
    }

    public async Task<List<Guid>> GetExistingProductIdsAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        List<CatalogProduct> products = await _catalog.GetProductsAsync(productIds, cancellationToken);
        return products.Select(product => product.Id).ToList();
    }
}

internal sealed class ProductInventoryAdapter : IProductInventory
{
    private readonly IInventoryQuery _inventory;

    public ProductInventoryAdapter(IInventoryQuery inventory)
    {
        _inventory = inventory;
    }

    public async Task<List<ProductInventory>> GetStocksAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        List<ProductStock> stocks = await _inventory.GetStocksAsync(productIds, cancellationToken);
        return stocks.Select(stock => new ProductInventory(stock.ProductId, stock.Quantity, stock.ReservedQuantity)).ToList();
    }
}
