using CommonModule.BusinessExceptions;
using PaymentRefundModule.Application.Ports;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Application.UseCases.CheckoutPayment;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using Xunit;

namespace PaymentRefundModule.Tests;

public sealed class CheckoutPaymentUrlTests
{
    [Theory]
    [InlineData("MOMO")]
    [InlineData("VNPAY")]
    public async Task ReturnsStoredUrlWithoutCallingGateway(string gatewayCode)
    {
        Repository repository = new Repository(NewPayment(gatewayCode));
        repository.Payment.SetPaymentUrl("https://gateway.example/pay/existing");
        Gateway gateway = new Gateway(repository, false, true);

        CheckoutPayment_UCOutput result = await CreateHandler(repository, gateway).Handle(
            new CheckoutPayment_UCInput(Guid.NewGuid(), repository.Payment.Id), default);

        Assert.Equal("https://gateway.example/pay/existing", result.PaymentUrl);
        Assert.Equal(0, gateway.ResolveCount);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task PaidPaymentCannotReuseStoredUrl()
    {
        Repository repository = new Repository(NewPayment("MOMO"));
        repository.Payment.SetPaymentUrl("https://gateway.example/pay/existing");
        repository.Payment.ConfirmOnline("paid-reference");
        Gateway gateway = new Gateway(repository, false, true);

        await Assert.ThrowsAsync<CommonModule.Domain.Exceptions.DomainException>(() =>
            CreateHandler(repository, gateway).Handle(
                new CheckoutPayment_UCInput(Guid.NewGuid(), repository.Payment.Id), default));

        Assert.Equal(0, gateway.ResolveCount);
    }

    [Theory]
    [InlineData("MOMO", false)]
    [InlineData("VNPAY", false)]
    [InlineData("MOMO", true)]
    public async Task StoresUrlWithoutOverwritingCallbackStatus(string gatewayCode, bool callbackArrives)
    {
        Repository repository = new Repository(NewPayment(gatewayCode));
        Gateway gateway = new Gateway(repository, callbackArrives, false);
        CheckoutPayment_UCHandler handler = CreateHandler(repository, gateway);

        CheckoutPayment_UCOutput result = await handler.Handle(
            new CheckoutPayment_UCInput(Guid.NewGuid(), repository.Payment.Id), default);

        Assert.Equal("https://gateway.example/pay/test", repository.Payment.PaymentUrl);
        Assert.Equal(repository.Payment.PaymentUrl, result.PaymentUrl);
        Assert.Equal(1, repository.UpdateCount);
        Assert.Equal(callbackArrives ? TransactionStatus.SUCCESS : TransactionStatus.PENDING, repository.Payment.Status);
        if (callbackArrives)
        {
            Assert.Equal("paid-reference", repository.Payment.TransactionRef);
        }
    }

    [Fact]
    public async Task DoesNotSaveUrlWhenGatewayFails()
    {
        Repository repository = new Repository(NewPayment("MOMO"));
        CheckoutPayment_UCHandler handler = CreateHandler(repository, new Gateway(repository, false, true));

        await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(
            new CheckoutPayment_UCInput(Guid.NewGuid(), repository.Payment.Id), default));

        Assert.Null(repository.Payment.PaymentUrl);
        Assert.Equal(0, repository.UpdateCount);
    }

    private static Payment NewPayment(string gatewayCode)
    {
        Guid methodId = Guid.NewGuid();
        return new Payment(PaymentType.ORDER, Guid.NewGuid(), methodId, 10000,
            new TransactionMethod(methodId, gatewayCode, TransactionMethodType.ONLINE, gatewayCode));
    }

    private static CheckoutPayment_UCHandler CreateHandler(Repository repository, Gateway gateway)
    {
        Ownership ownership = new Ownership();
        return new CheckoutPayment_UCHandler(repository,
            new PaymentOwnershipService(ownership, ownership), gateway, new Transaction());
    }
    private sealed class Ownership : IOrderOwnershipPort, IRepairOwnershipPort
    {
        public Task<bool> IsOwnedByCustomerAsync(Guid refId, Guid customerId, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class Transaction : IExecuteAtomically
    {
        public Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> mainTask, CancellationToken cancellationToken = default) => mainTask();
    }

    private sealed class Gateway : IPaymentGatewayFactory, IPaymentGatewayStrategy
    {
        private readonly Repository _repository;
        private readonly bool _callbackArrives;
        private readonly bool _fails;
        public string GatewayCode => "TEST";

        public int ResolveCount { get; private set; }
        public Gateway(Repository repository, bool callbackArrives, bool fails)
        {
            _repository = repository;
            _callbackArrives = callbackArrives;
            _fails = fails;
        }
        public Task<IPaymentGatewayStrategy> GetStrategyAsync(Guid transactionMethodId, CancellationToken cancellationToken = default)
        {
            ResolveCount++;
            return Task.FromResult<IPaymentGatewayStrategy>(this);
        }
        public Task<PaymentCheckoutResult> CheckoutAsync(PaymentCheckoutRequest request, CancellationToken cancellationToken = default)
        {
            if (_fails)
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, "Test gateway failure");
            }
            if (_callbackArrives)
            {
                Payment original = _repository.Payment;
                Payment refreshed = new Payment(original.Id, original.Type, original.OrderId, original.RepairRequestId,
                    original.TransactionMethodId, original.Status, original.Amount, original.TransactionRef,
                    original.FailureReason, original.CompletedAt, original.CreatedAt, original.UpdatedAt, original.TransactionMethod);
                refreshed.ConfirmOnline("paid-reference");
                _repository.Payment = refreshed;
            }
            return Task.FromResult(new PaymentCheckoutResult("https://gateway.example/pay/test", DateTimeOffset.UtcNow.AddMinutes(15)));
        }
    }

    private sealed class Repository : IPaymentRepository
    {
        public Payment Payment { get; set; }
        public int UpdateCount { get; private set; }
        public Repository(Payment payment)
        {
            Payment = payment;
        }
        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Payment?>(Payment);
        public Task<Payment?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Payment?>(Payment);
        public Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            Payment = payment;
            UpdateCount++;
            return Task.CompletedTask;
        }
        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize, PaymentType? type, TransactionStatus? status, Guid? refId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<Payment>> GetByRefIdAsync(Guid refId, PaymentType type, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

}
