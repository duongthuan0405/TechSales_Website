namespace PaymentRefundModule.Application.UseCases.GetTransactionMethods;

internal sealed record GetTransactionMethods_UCOutput(List<GetTransactionMethods_UCOutput.TransactionMethodInfo> TransactionMethods)
{
    internal sealed record TransactionMethodInfo(string Id, string Name, string Type);
}
