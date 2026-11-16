namespace AfterSaleModule.Infrastructure.Persistence.Models;

internal sealed class ReturnRequestReasonDbModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<ReturnRequestDbModel> ReturnRequests { get; set; } = new HashSet<ReturnRequestDbModel>();
}
