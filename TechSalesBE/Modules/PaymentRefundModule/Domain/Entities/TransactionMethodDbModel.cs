using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Domain.Entities;

internal sealed class TransactionMethod
{
    private Guid _id= Guid.Empty;
    private string name = string.Empty;
    private TransactionMethodType type;

    public TransactionMethod(Guid id, string name, TransactionMethodType type)
    {
        _id = id;
        this.name = name;
        this.type = type;
    }

    public Guid Id { get => _id; set => _id = value; }
    public string Name { get => name; set => name = value; }
    internal TransactionMethodType Type { get => type; set => type = value; }
}
