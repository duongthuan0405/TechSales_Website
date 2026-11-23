using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using CatalogModule.Infrastructure.Persistence;
using CatalogModule.Infrastructure.Persistence.Models;
using CatalogModule.Application.Exceptions;
using CommonModule.BusinessExceptions;
using Microsoft.EntityFrameworkCore;
using CatalogModule.Domain.Enums;

namespace CatalogModule.Infrastructure.Repositories;

internal sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _db;

    public ProductRepository(CatalogDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        ProductDbModel row = new ProductDbModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Status = product.Status,
            BrandId = product.BrandId,
            CategoryId = product.CategoryId,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
        foreach (ProductImage image in product.Images)
        {
            row.Images.Add(new ProductImageDbModel
            {
                Id = image.Id,
                ProductId = product.Id,
                ImageUrl = image.ImageUrl,
                IsPrimary = image.IsPrimary,
                CreatedAt = image.CreatedAt
            });
        }

        await _db.Products.AddAsync(row, cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        ProductDbModel? row = await _db.Products.AsNoTracking().Include(product => product.Images)
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return Map(row);
    }

    private static Product Map(ProductDbModel row)
    {
        List<ProductImage> images = row.Images.OrderBy(image => image.CreatedAt).ThenBy(image => image.Id)
            .Select(image => ProductImage.Restore(image.Id, image.ProductId, image.ImageUrl,
                image.IsPrimary.GetValueOrDefault(), image.CreatedAt)).ToList();
        return Product.Restore(row.Id, row.Name, row.Description, row.Price, row.Status,
            row.BrandId, row.CategoryId, row.CreatedAt, row.UpdatedAt, images);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize,
        string? keyword, List<Guid> keywordBrandIds, Guid? categoryId, ProductStatus? status,
        Guid? productId, Guid? brandId, CancellationToken cancellationToken)
    {
        IQueryable<ProductDbModel> query = _db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string term = keyword.Trim().ToLowerInvariant();
            query = query.Where(product => product.Name.ToLower().Contains(term)
                || product.Description.ToLower().Contains(term) || keywordBrandIds.Contains(product.BrandId));
        }
        if (brandId.HasValue)
        {
            query = query.Where(product => product.BrandId == brandId.Value);
        }
        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }
        if (status.HasValue)
        {
            query = query.Where(product => product.Status == status.Value);
        }
        if (productId.HasValue)
        {
            query = query.Where(product => product.Id == productId.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        List<ProductDbModel> rows = await query.Include(product => product.Images)
            .OrderByDescending(product => product.CreatedAt).ThenBy(product => product.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (rows.Select(Map).ToList(), totalCount);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        ProductDbModel? row = await _db.Products.SingleOrDefaultAsync(item => item.Id == product.Id, cancellationToken);
        if (row == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProductNotFound);
        }

        row.Name = product.Name;
        row.Description = product.Description;
        row.Price = product.Price;
        row.Status = product.Status;
        row.BrandId = product.BrandId;
        row.CategoryId = product.CategoryId;
        row.UpdatedAt = product.UpdatedAt;
    }

    public async Task UpdateImagesAsync(Product product, CancellationToken cancellationToken)
    {
        ProductDbModel? row = await _db.Products.Include(item => item.Images)
            .SingleOrDefaultAsync(item => item.Id == product.Id, cancellationToken);
        if (row == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProductNotFound);
        }
        List<Guid> retainedIds = product.Images.Select(image => image.Id).ToList();
        foreach (ProductImageDbModel removed in row.Images.Where(image => !retainedIds.Contains(image.Id)).ToList())
        {
            _db.ProductImages.Remove(removed);
        }
        foreach (ProductImage image in product.Images)
        {
            ProductImageDbModel? stored = row.Images.SingleOrDefault(item => item.Id == image.Id);
            if (stored == null)
            {
                stored = new ProductImageDbModel
                {
                    Id = image.Id, ProductId = product.Id, ImageUrl = image.ImageUrl, CreatedAt = image.CreatedAt
                };
                _db.ProductImages.Add(stored);
            }
            stored.IsPrimary = image.IsPrimary;
        }
        row.UpdatedAt = product.UpdatedAt;
    }
}
