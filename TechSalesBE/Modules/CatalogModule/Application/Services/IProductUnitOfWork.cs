namespace CatalogModule.Application.Services;

internal interface IProductUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
