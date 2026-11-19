using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;
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

    public async Task<List<PaymentMethod>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<PaymentMethodDbModel> dbModels = await _dbContext.PaymentMethods
            .AsNoTracking()
            .OrderBy(paymentMethod => paymentMethod.name)
            .ThenBy(paymentMethod => paymentMethod.id)
            .ToListAsync(cancellationToken);

        List<PaymentMethod> paymentMethods = new List<PaymentMethod>();
        foreach (PaymentMethodDbModel dbModel in dbModels)
        {
            paymentMethods.Add(MapToEntity(dbModel));
        }

        return paymentMethods;
    }

    public async Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        PaymentMethodDbModel? dbModel = await _dbContext.PaymentMethods
            .AsNoTracking()
            .SingleOrDefaultAsync(paymentMethod => paymentMethod.id == id, cancellationToken);

        if (dbModel == null)
        {
            return null;
        }

        return MapToEntity(dbModel);
    }

    public async Task AddAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paymentMethod);
        cancellationToken.ThrowIfCancellationRequested();

        if (paymentMethod.id == Guid.Empty)
        {
            paymentMethod.id = Guid.NewGuid();
        }

        PaymentMethodDbModel dbModel = new PaymentMethodDbModel
        {
            id = paymentMethod.id,
            name = paymentMethod.name,
            type = paymentMethod.type
        };
        await _dbContext.PaymentMethods.AddAsync(dbModel, cancellationToken);
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PaymentMethods.AnyAsync(cancellationToken);
    }

    private static PaymentMethod MapToEntity(PaymentMethodDbModel dbModel)
    {
        PaymentMethodType type = PaymentMethodType.CASH;
        if (dbModel.type.HasValue)
        {
            type = dbModel.type.Value;
        }

        return new PaymentMethod
        {
            id = dbModel.id,
            name = dbModel.name ?? string.Empty,
            type = type
        };
    }
}
