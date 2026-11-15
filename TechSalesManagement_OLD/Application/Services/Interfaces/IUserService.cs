using System;
using System.Threading.Tasks;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetByIdAsync(GetUserByIdParams parameters);
}
