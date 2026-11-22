using ProductModule.Application.Repositories;
using ProductModule.Domain.Entities;
using ProductModule.Infrastructure.Persistence;
using ProductModule.Infrastructure.Persistence.Models;
using ProductModule.Application.Exceptions;
using CommonModule.BusinessExceptions;
using Microsoft.EntityFrameworkCore;
using ProductModule.Domain.Enums;

namespace ProductModule.Infrastructure.Repositories;

internal sealed class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _db;

    public ProductRepository(ProductDbContext db)
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
        Guid? productId, CancellationToken cancellationToken)
    {
        IQueryable<ProductDbModel> query = _db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string term = keyword.Trim().ToLowerInvariant();
            query = query.Where(product => product.Name.ToLower().Contains(term)
                || product.Description.ToLower().Contains(term) || keywordBrandIds.Contains(product.BrandId));
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
}
