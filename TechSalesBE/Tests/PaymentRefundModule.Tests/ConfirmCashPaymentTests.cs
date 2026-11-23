using CommonModule.BusinessExceptions;
using CommonModule.Domain.Exceptions;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Application.UseCases.ConfirmCashPayment;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using Xunit;

namespace PaymentRefundModule.Tests;

public sealed class ConfirmCashPaymentTests
{
    [Fact]
    public async Task ConfirmationIsIdempotentAndUpdatesOnlyOnceInsideTransaction()
    {
        Repository repository = new Repository(CreatePayment(TransactionStatus.PENDING));
        Transaction transaction = new Transaction(repository);
        ConfirmCashPayment_UCHandler handler = new ConfirmCashPayment_UCHandler(repository, new Methods(TransactionMethodType.CASH), transaction);
        ConfirmCashPayment_UCInput input = new ConfirmCashPayment_UCInput(repository.Payment!.Id);
        ConfirmCashPayment_UCOutput first = await handler.Handle(input, CancellationToken.None);
        ConfirmCashPayment_UCOutput second = await handler.Handle(input, CancellationToken.None);
        Assert.Equal("SUCCESS", first.Status);
        Assert.NotNull(first.CompletedAt);
        Assert.Equal(first.CompletedAt, first.UpdatedAt);
        Assert.Equal(first, second);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Theory]
    [InlineData(TransactionStatus.FAILED)]
    [InlineData(TransactionStatus.CANCELLED)]
    public async Task TerminalFailuresCannotBeConfirmed(TransactionStatus status)
    {
        Repository repository = new Repository(CreatePayment(status));
        ConfirmCashPayment_UCHandler handler = new ConfirmCashPayment_UCHandler(repository,
            new Methods(TransactionMethodType.CASH), new Transaction(repository));
        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new ConfirmCashPayment_UCInput(repository.Payment!.Id), CancellationToken.None));
        Assert.Equal(status, repository.Payment!.Status);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Theory]
    [InlineData(TransactionStatus.PENDING)]
    [InlineData(TransactionStatus.SUCCESS)]
    public async Task OnlinePaymentsCannotBeConfirmedAsCash(TransactionStatus status)
    {
        Repository repository = new Repository(CreatePayment(status));
        ConfirmCashPayment_UCHandler handler = new ConfirmCashPayment_UCHandler(repository,
            new Methods(TransactionMethodType.ONLINE), new Transaction(repository));
        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new ConfirmCashPayment_UCInput(repository.Payment!.Id), CancellationToken.None));
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task MissingPaymentReturnsNotFound()
    {
        Repository repository = new Repository(null);
        ConfirmCashPayment_UCHandler handler = new ConfirmCashPayment_UCHandler(repository,
            new Methods(TransactionMethodType.CASH), new Transaction(repository));
        BusinessException error = await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(new ConfirmCashPayment_UCInput(Guid.NewGuid()), CancellationToken.None));
        Assert.Equal(BusinessExceptionCode.NOT_FOUND, error.Code);
    }

    private static Payment CreatePayment(TransactionStatus status)
    {
        DateTimeOffset created = DateTimeOffset.UtcNow.AddMinutes(-1);
        return new Payment(Guid.NewGuid(), PaymentType.ORDER, Guid.NewGuid(), null, Guid.NewGuid(), status,
            100_000, null, null, null, created, created);
    }

    private sealed class Transaction : IExecuteAtomically
    {
        private readonly Repository _repository;
        public Transaction(Repository repository)
        {
            _repository = repository;
        }
        public async Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> mainTask, CancellationToken cancellationToken = default)
        {
            _repository.InTransaction = true;
            try
            {
                return await mainTask();
            }
            finally
            {
                _repository.InTransaction = false;
            }
        }
    }

    private sealed class Methods : ITransactionMethodRepository
    {
        private readonly TransactionMethodType _type;
        public Methods(TransactionMethodType type)
        {
            _type = type;
        }
        public Task<TransactionMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<TransactionMethod?>(new TransactionMethod(id, "Test", _type));
        public Task<List<TransactionMethod>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> AnyAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class Repository : IPaymentRepository
    {
        internal Payment? Payment { get; }
        internal int UpdateCount { get; private set; }
        internal bool InTransaction { get; set; }
        public Repository(Payment? payment)
        {
            Payment = payment;
        }
        public Task<Payment?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Assert.True(InTransaction);
            return Task.FromResult(Payment);
        }
        public Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            Assert.True(InTransaction);
            UpdateCount++;
            return Task.CompletedTask;
        }
        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<Payment>> GetByRefIdAsync(Guid refId, PaymentType type, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize, PaymentType? type,
            TransactionStatus? status, Guid? refId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
