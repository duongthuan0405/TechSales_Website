using System;
using System.Collections.Generic;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.HelperServices;

public interface IJwtService
{
    string GenerateToken(User user, IEnumerable<Role> roles);
    Guid? ValidateToken(string token);
}
