using System;
using System.Collections.Generic;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class CategoryDbModel
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public DateTimeOffset created_at { get; set; }

    // Navigation collections
    public ICollection<ProductDbModel> products { get; set; } = new HashSet<ProductDbModel>();
}
