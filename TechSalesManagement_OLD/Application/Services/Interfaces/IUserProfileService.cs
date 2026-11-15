using System;
using System.Threading.Tasks;
using TechSalesManagement.Application.Services.Params;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IUserProfileService
{
    Task UpdateProfileAsync(UpdateProfileParams parameters);
}
