using System;
using TechSalesManagement.Domain.Common;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Domain.Entities;

public class Product
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal price { get; set; }
    public ProductStatus status { get; set; } = ProductStatus.ACTIVE;
    public string brand { get; set; } = string.Empty;
    public Guid categoryId { get; set; }

    public List<ProductImage> images { get; set; } = new();
    public Inventory? inventory { get; set; }
    public double rating { get; set; }

    public Product(string name, decimal price, Guid categoryId)
    {
        this.name = name;
        this.price = price;
        this.categoryId = categoryId;
    }

    public Product() { }

    public void UpdateInfo(string name, string description, decimal price, string brand, Guid categoryId)
    {
        this.name = name;
        this.description = description;
        this.price = price;
        this.brand = brand;
        this.categoryId = categoryId;
        this.updatedAt = DateTimeOffset.UtcNow;
    }

    public void Discontinue()
    {
        this.status = ProductStatus.DISCONTINUED;
        this.updatedAt = DateTimeOffset.UtcNow;
    }

    public class Builder
    {
        private readonly Product _product = new();

        public Builder WithBasicInfo(string name, string description, decimal price, string brand, Guid categoryId)
        {
            _product.name = name;
            _product.description = description;
            _product.price = price;
            _product.brand = brand;
            _product.categoryId = categoryId;
            return this;
        }

        public Builder WithImages(List<ProductImage> images)
        {
            _product.images = images ?? new();
            // BR170: Ensure at least one primary image
            if (_product.images.Any() && !_product.images.Any(img => img.isPrimary))
            {
                _product.images[0].isPrimary = true;
            }
            return this;
        }

        public Builder WithInventory(int initialQuantity)
        {
            _product.inventory = new Inventory(_product.id, initialQuantity);
            return this;
        }

        public Product Build()
        {
            if (string.IsNullOrWhiteSpace(_product.name)) throw new InvalidOperationException("Product name is required.");
            if (_product.price < 0) throw new InvalidOperationException("Price cannot be negative.");
            if (_product.categoryId == Guid.Empty) throw new InvalidOperationException("Category is required.");
            
            _product.id = Guid.NewGuid();
            _product.createdAt = DateTimeOffset.UtcNow;
            _product.status = ProductStatus.ACTIVE;
            return _product;
        }
    }
}
