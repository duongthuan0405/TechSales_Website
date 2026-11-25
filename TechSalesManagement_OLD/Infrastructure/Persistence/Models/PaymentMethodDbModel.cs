using System;
using System.Collections.Generic;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class PaymentMethodDbModel
{
    public Guid id { get; set; }
    public string? name { get; set; }
    public PaymentMethodType? type { get; set; }

    // Navigation collections
    public ICollection<PaymentDbModel> payments { get; set; } = new HashSet<PaymentDbModel>();
}
