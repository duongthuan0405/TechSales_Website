using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Domain.Entities;

internal sealed class TransactionMethod
{
    private Guid _id= Guid.Empty;
    private string name = string.Empty;
    private TransactionMethodType type;
    private string _code;

    public TransactionMethod(Guid id, string name, TransactionMethodType type, string code = null)
    {
        Id = id;
        Name = name;
        Type = type;
        Code = code;
    }

    public Guid Id { get => _id; set => _id = value; }
    public string Name { get => name; set => name = value; }
    public string Code { get => _code; set => _code = value; }
    internal TransactionMethodType Type { get => type; set => type = value; }
}
