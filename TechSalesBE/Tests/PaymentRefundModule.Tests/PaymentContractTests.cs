using CommonModule.Application.Behaviors;
using CommonModule.BusinessExceptions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PaymentRefundModule.Application.Ports;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Presentation.Contracts.CreatePayment;
using PaymentRefundModule.Presentation.Contracts.ReplacePayment;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using Xunit;

namespace PaymentRefundModule.Tests;

public sealed class PaymentContractTests
{
    [Fact]
    public async Task CreateThenReplaceUsesSameUseCasesAndKeepsOnePending()
    {
        Repository repository = new Repository();
        using ServiceProvider provider = BuildProvider(repository);
        IPaymentCreateContract create = provider.GetRequiredService<IPaymentCreateContract>();
        IPaymentReplaceContract replace = provider.GetRequiredService<IPaymentReplaceContract>();
        Guid customer = Guid.NewGuid();
        Guid reference = Guid.NewGuid();
        CreatePaymentResult first = await create.CreatePaymentAsync(customer, "ORDER", reference, Methods.Cash);
        BusinessException duplicate = await Assert.ThrowsAsync<BusinessException>(() => create.CreatePaymentAsync(customer, "ORDER", reference, Methods.Cash));
        Assert.Equal(BusinessExceptionCode.CONFLICT, duplicate.Code);
        ReplacePaymentResult second = await replace.ReplacePaymentAsync(customer, "order", reference, Methods.Cash);
        Assert.Equal(first.PaymentId, second.ReplacedPaymentId);
        Assert.NotEqual(first.PaymentId, second.PaymentId);
        Assert.Equal(TransactionStatus.CANCELLED, repository.Items.Single(item => item.Id == first.PaymentId).Status);
        Assert.Single(repository.Items.Where(item => item.Status == TransactionStatus.PENDING));
        Assert.Equal(100_000, repository.Items.Single(item => item.Id == second.PaymentId).Amount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BothContractsValidateSharedInput(bool replace)
    {
        Repository repository = new Repository();
        using ServiceProvider provider = BuildProvider(repository);
        Func<Task> call = async () =>
        {
            if (replace)
            {
                await provider.GetRequiredService<IPaymentReplaceContract>().ReplacePaymentAsync(Guid.Empty, "INVALID", Guid.Empty, Guid.Empty);
            }
            else
            {
                await provider.GetRequiredService<IPaymentCreateContract>().CreatePaymentAsync(Guid.Empty, "INVALID", Guid.Empty, Guid.Empty);
            }
        };
        BusinessException error = await Assert.ThrowsAsync<BusinessException>(call);
        Assert.Equal(BusinessExceptionCode.BAD_REQUEST, error.Code);
        Assert.Empty(repository.Items);
        Assert.Equal(0, repository.LockCount);
    }

    [Fact]
    public async Task PaidReferenceRejectsCreateAndReplace()
    {
        Repository repository = new Repository();
        Payment payment = new Payment(PaymentType.ORDER, Guid.NewGuid(), Methods.Cash, 100_000);
        payment.ConfirmCash(TransactionMethodType.CASH);
        repository.Items.Add(payment);
        using ServiceProvider provider = BuildProvider(repository);
        await Assert.ThrowsAsync<BusinessException>(() => provider.GetRequiredService<IPaymentCreateContract>()
            .CreatePaymentAsync(Guid.NewGuid(), "ORDER", payment.OrderId!.Value, Methods.Cash));
        await Assert.ThrowsAsync<BusinessException>(() => provider.GetRequiredService<IPaymentReplaceContract>()
            .ReplacePaymentAsync(Guid.NewGuid(), "ORDER", payment.OrderId!.Value, Methods.Cash));
        Assert.Single(repository.Items);
        Assert.Equal(TransactionStatus.SUCCESS, payment.Status);
    }

    [Fact]
    public async Task CannotReplaceOnlinePendingWithoutCancellingGatewaySession()
    {
        Repository repository = new Repository();
        Payment payment = new Payment(PaymentType.ORDER, Guid.NewGuid(), Methods.Online, 100_000);
        repository.Items.Add(payment);
        using ServiceProvider provider = BuildProvider(repository);
        BusinessException error = await Assert.ThrowsAsync<BusinessException>(() => provider.GetRequiredService<IPaymentReplaceContract>()
            .ReplacePaymentAsync(Guid.NewGuid(), "ORDER", payment.OrderId!.Value, Methods.Cash));
        Assert.Equal(BusinessExceptionCode.CONFLICT, error.Code);
        Assert.Equal(TransactionStatus.PENDING, payment.Status);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task InvalidNewMethodLeavesPreviousPaymentPending()
    {
        Repository repository = new Repository();
        Payment payment = new Payment(PaymentType.REPAIR, Guid.NewGuid(), Methods.Cash, 100_000);
        repository.Items.Add(payment);
        using ServiceProvider provider = BuildProvider(repository);
        await Assert.ThrowsAsync<BusinessException>(() => provider.GetRequiredService<IPaymentReplaceContract>()
            .ReplacePaymentAsync(Guid.NewGuid(), "REPAIR", payment.RepairRequestId!.Value, Guid.NewGuid()));
        Assert.Equal(TransactionStatus.PENDING, payment.Status);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task ReplaceWithoutPendingCreatesNewPayment()
    {
        Repository repository = new Repository();
        using ServiceProvider provider = BuildProvider(repository);
        ReplacePaymentResult result = await provider.GetRequiredService<IPaymentReplaceContract>()
            .ReplacePaymentAsync(Guid.NewGuid(), "REPAIR", Guid.NewGuid(), Methods.Cash);
        Assert.Null(result.ReplacedPaymentId);
        Assert.Equal(result.PaymentId, Assert.Single(repository.Items).Id);
    }

    private static ServiceProvider BuildProvider(Repository repository)
    {
        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(PaymentCreationService).Assembly);
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(PaymentCreationService).Assembly, includeInternalTypes: true);
        services.AddSingleton<IPaymentRepository>(repository);
        services.AddSingleton<ITransactionMethodRepository, Methods>();
        services.AddSingleton<IOrderOwnershipPort, Source>();
        services.AddSingleton<IRepairOwnershipPort, Source>();
        services.AddSingleton<IPaymentAmountPort, Source>();
        services.AddSingleton<IExecuteAtomically>(new Transaction(repository));
        services.AddScoped<PaymentOwnershipService>();
        services.AddScoped<PaymentCreationService>();
        services.AddScoped<IPaymentCreateContract, PaymentCreateContract>();
        services.AddScoped<IPaymentReplaceContract, PaymentReplaceContract>();
        return services.BuildServiceProvider();
    }

    private sealed class Source : IOrderOwnershipPort, IRepairOwnershipPort, IPaymentAmountPort
    {
        public Task<bool> IsOwnedByCustomerAsync(Guid refId, Guid customerId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<long> GetAmountAsync(PaymentType type, Guid refId, CancellationToken cancellationToken = default) => Task.FromResult(100_000L);
    }

    private sealed class Methods : ITransactionMethodRepository
    {
        internal static readonly Guid Cash = Guid.NewGuid();
        internal static readonly Guid Online = Guid.NewGuid();
        public Task<TransactionMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            TransactionMethod? method = null;
            if (id == Cash || id == Online)
            {
                method = new TransactionMethod(id, "Test", id == Cash ? TransactionMethodType.CASH : TransactionMethodType.ONLINE);
            }
            return Task.FromResult(method);
        }
        public Task<List<TransactionMethod>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> AnyAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
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

    private sealed class Repository : IPaymentRepository
    {
        internal List<Payment> Items { get; } = new List<Payment>();
        internal bool InTransaction { get; set; }
        internal int LockCount { get; private set; }
        public Task LockReferenceAsync(PaymentType type, Guid refId, CancellationToken cancellationToken = default)
        {
            Assert.True(InTransaction);
            LockCount++;
            return Task.CompletedTask;
        }
        public Task<Payment?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Assert.True(InTransaction);
            return Task.FromResult(Items.SingleOrDefault(item => item.Id == id));
        }
        public Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            Assert.True(InTransaction);
            return Task.CompletedTask;
        }
        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            Assert.True(InTransaction);
            Items.Add(payment);
            return Task.CompletedTask;
        }
        public Task<List<Payment>> GetByRefIdAsync(Guid refId, PaymentType type, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Where(item => item.Type == type && (item.OrderId == refId || item.RepairRequestId == refId)).ToList());
        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize, PaymentType? type,
            TransactionStatus? status, Guid? refId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
