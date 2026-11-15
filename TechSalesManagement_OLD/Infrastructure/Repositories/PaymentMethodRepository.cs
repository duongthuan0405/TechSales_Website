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

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly TechSalesDbContext _dbContext;

    public PaymentMethodRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PaymentMethod>> GetAllAsync()
    {
        var dbModels = await _dbContext.PaymentMethods.ToListAsync();
        return dbModels.Select(m => MapToEntity(m)).ToList();
    }

    public async Task<PaymentMethod?> GetByIdAsync(Guid id)
    {
        var dbModel = await _dbContext.PaymentMethods.FindAsync(id);
        return dbModel != null ? MapToEntity(dbModel) : null;
    }

    public async Task AddAsync(PaymentMethod paymentMethod)
    {
        var dbModel = new PaymentMethodDbModel
        {
            id = paymentMethod.id == Guid.Empty ? Guid.NewGuid() : paymentMethod.id,
            name = paymentMethod.name,
            type = paymentMethod.type
        };
        await _dbContext.PaymentMethods.AddAsync(dbModel);
    }

    public async Task<bool> AnyAsync()
    {
        return await _dbContext.PaymentMethods.AnyAsync();
    }

    private PaymentMethod MapToEntity(PaymentMethodDbModel dbModel)
    {
        return new PaymentMethod
        {
            id = dbModel.id,
            name = dbModel.name ?? string.Empty,
            type = dbModel.type ?? TechSalesManagement.Domain.Enums.PaymentMethodType.CASH
        };
    }
}
