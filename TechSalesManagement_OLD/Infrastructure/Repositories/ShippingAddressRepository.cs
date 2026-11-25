using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class ShippingAddressRepository : IShippingAddressRepository
{
    private readonly TechSalesDbContext _dbContext;

    public ShippingAddressRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShippingAddress?> GetByIdAsync(Guid id)
    {
        var dbModel = await _dbContext.ShippingAddresses
            .FirstOrDefaultAsync(x => x.id == id && x.deleted_at == null);
            
        return MapToEntity(dbModel);
    }

    public async Task<ShippingAddress?> GetDefaultAddressByUserIdAsync(Guid userId)
    {
        var dbModel = await _dbContext.ShippingAddresses
            .FirstOrDefaultAsync(x => x.user_id == userId && x.is_default && x.deleted_at == null);
            
        return MapToEntity(dbModel);
    }

    public async Task<List<ShippingAddress>> GetAddressesByUserIdAsync(Guid userId)
    {
        var dbModels = await _dbContext.ShippingAddresses
            .Where(x => x.user_id == userId && x.deleted_at == null)
            .ToListAsync();
            
        return dbModels.Select(MapToEntity).Cast<ShippingAddress>().ToList();
    }

    public async Task AddAsync(ShippingAddress address)
    {
        address.id = Guid.NewGuid();
        address.createdAt = DateTimeOffset.UtcNow;

        var dbModel = MapToDbModel(address);
        await _dbContext.ShippingAddresses.AddAsync(dbModel);
    }

    public async Task UpdateAsync(ShippingAddress address)
    {
        var dbModel = await _dbContext.ShippingAddresses
            .FirstOrDefaultAsync(x => x.id == address.id);
            
        if (dbModel != null)
        {
            dbModel.province = address.province;
            dbModel.ward = address.ward;
            dbModel.detail = address.detail;
            dbModel.is_default = address.isDefault;
            dbModel.deleted_at = address.deletedAt;
            dbModel.updated_at = DateTimeOffset.UtcNow;
            
            _dbContext.ShippingAddresses.Update(dbModel);
        }
    }

    private ShippingAddress? MapToEntity(ShippingAddressDbModel? dbModel)
    {
        if (dbModel == null) return null;
        
        return new ShippingAddress
        {
            id = dbModel.id,
            userId = dbModel.user_id,
            province = dbModel.province,
            ward = dbModel.ward,
            detail = dbModel.detail,
            isDefault = dbModel.is_default,
            createdAt = dbModel.created_at,
            updatedAt = dbModel.updated_at,
            deletedAt = dbModel.deleted_at
        };
    }

    private ShippingAddressDbModel MapToDbModel(ShippingAddress address)
    {
        return new ShippingAddressDbModel
        {
            id = address.id,
            user_id = address.userId,
            province = address.province,
            ward = address.ward,
            detail = address.detail,
            is_default = address.isDefault,
            created_at = address.createdAt,
            updated_at = address.updatedAt,
            deleted_at = address.deletedAt
        };
    }
}
