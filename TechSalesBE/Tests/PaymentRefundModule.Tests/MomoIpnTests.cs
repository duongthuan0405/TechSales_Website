using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Http.Json;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using CommonModule.BusinessExceptions;
using CommonModule.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Application.UseCases.ProcessPaymentNotification;
using PaymentRefundModule.Application.UseCases.ReceiveMomoIpn;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Infrastructure.Payments.Options;
using PaymentRefundModule.Infrastructure.Payments;
using PaymentRefundModule.Infrastructure.Payments.Momo;
using PaymentRefundModule.Presentation.Controllers;
using Xunit;
using PaymentRefundModule.Application.UseCases.PollMomoPayment;

namespace PaymentRefundModule.Tests;

public sealed class MomoIpnTests
{
    [Theory]
    [InlineData(0, 10000, true)]
    [InlineData(1000, 10000, false)]
    public async Task PollingUpdatesOnlyConfirmedSuccessAndIsIdempotent(int code, long amount, bool expected)
    {
        Payment payment = NewPayment();
        Repository repository = new Repository(payment);
        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(MomoCallbackController).Assembly));
        services.AddSingleton<IPaymentRepository>(repository);
        services.AddSingleton<IExecuteAtomically, Transaction>();
        services.AddSingleton<IMomoStatusQuery>(new StatusQuery(payment.Id, amount, code));
        using ServiceProvider provider = services.BuildServiceProvider();
        ISender sender = provider.GetRequiredService<ISender>();

        PollMomoPayment_UCOutput result = await sender.Send(new PollMomoPayment_UCInput(payment.Id));
        Assert.Equal(expected, result.Updated);
        Assert.False((await sender.Send(new PollMomoPayment_UCInput(payment.Id))).Updated);
        Assert.Equal(expected ? 1 : 0, repository.UpdateCount);
    }

    [Fact]
    public async Task PollingRejectsAmountMismatch()
    {
        Payment payment = NewPayment();
        PollMomoPayment_UCHandler handler = new PollMomoPayment_UCHandler(
            new Repository(payment), new StatusQuery(payment.Id, payment.Amount + 1, 0), null!);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new PollMomoPayment_UCInput(payment.Id), default));
        Assert.Equal(TransactionStatus.PENDING, payment.Status);
    }

    private sealed class StatusQuery : IMomoStatusQuery
    {
        private readonly MomoStatusResult _result;
        public StatusQuery(Guid id, long amount, int code)
        {
            _result = new MomoStatusResult(id.ToString("N"), amount, 123456, code);
        }
        public Task<MomoStatusResult> QueryAsync(Guid paymentId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }

    [Theory]
    [InlineData("MOMO")]
    [InlineData("OTHER_GATEWAY")]
    public async Task SharedHandlerProcessesVerifiedNotificationsFromDifferentGateways(string gatewayCode)
    {
        Payment payment = NewPayment(gatewayCode);
        Repository repository = new Repository(payment);
        ProcessPaymentNotification_UCHandler handler = new ProcessPaymentNotification_UCHandler(
            repository, new Transaction(),
            NullLogger<ProcessPaymentNotification_UCHandler>.Instance);
        ProcessPaymentNotification_UCInput input = new ProcessPaymentNotification_UCInput(
            payment.Id, payment.Amount, gatewayCode, "gateway-transaction-1", PaymentNotificationStatus.Success);

        Assert.True((await handler.Handle(input, default)).Updated);
        Assert.False((await handler.Handle(input, default)).Updated);
        Assert.Equal(TransactionStatus.SUCCESS, payment.Status);
        Assert.Equal("gateway-transaction-1", payment.TransactionRef);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task EndpointAcceptsAnonymousIpnAndReturnsNoContentForDuplicate()
    {
        Payment payment = NewPayment();
        Repository repository = new Repository(payment);
        using TestServer server = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddControllers().AddApplicationPart(typeof(MomoCallbackController).Assembly);
                services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(MomoCallbackController).Assembly));
                services.AddSingleton<IOptions<MomoOptions>>(Options.Create(Settings));
                services.AddSingleton<IPaymentRepository>(repository);
                services.AddSingleton<ITransactionMethodRepository>(new Methods(payment.TransactionMethodId, "MOMO"));
                services.AddSingleton<IExecuteAtomically, Transaction>();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            }));
        using HttpClient client = server.CreateClient();
        ReceiveMomoIpn_UCInput input = CreateNotification(payment.Id);

        using HttpResponseMessage first = await client.PostAsJsonAsync("/api/payments/momo/ipn", input);
        using HttpResponseMessage duplicate = await client.PostAsJsonAsync("/api/payments/momo/ipn", input);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, duplicate.StatusCode);
        Assert.Equal(1, repository.UpdateCount);
    }

    private static readonly MomoOptions Settings = new MomoOptions
    {
        PartnerCode = "TEST", AccessKey = "access", SecretKey = "secret"
    };

    [Fact]
    public void VerifierAcceptsValidSignatureAndRejectsTampering()
    {
        ReceiveMomoIpn_UCInput input = CreateNotification(Guid.NewGuid());
        ReceiveMomoIpn_UCHandler verifier = new ReceiveMomoIpn_UCHandler(Options.Create(Settings), null!);

        Assert.True(verifier.IsValid(input));
        Assert.False(verifier.IsValid(input with { Amount = 20000 }));
        Assert.False(verifier.IsValid(input with { PartnerCode = "OTHER" }));
        Assert.False(verifier.IsValid(input with { Signature = new string('z', 64) }));
        Assert.False(new ReceiveMomoIpn_UCHandler(Options.Create(new MomoOptions()), null!).IsValid(input));
    }

    [Fact]
    public async Task SuccessSetsReferenceAndDuplicateDoesNotUpdateAgain()
    {
        Payment payment = NewPayment();
        Repository repository = new Repository(payment);
        ReceiveMomoIpn_UCHandler handler = CreateHandler(repository);
        ReceiveMomoIpn_UCInput input = CreateNotification(payment.Id);

        Assert.True((await handler.Handle(input, default)).Updated);
        DateTimeOffset? completedAt = payment.CompletedAt;
        Assert.False((await handler.Handle(input, default)).Updated);
        Assert.Equal(TransactionStatus.SUCCESS, payment.Status);
        Assert.Equal("123456", payment.TransactionRef);
        Assert.Equal(completedAt, payment.CompletedAt);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Theory]
    [InlineData("amount")]
    [InlineData("provider")]
    [InlineData("signature")]
    public async Task InvalidNotificationDoesNotChangePayment(string invalidField)
    {
        Payment payment = NewPayment(invalidField == "provider" ? "OTHER" : "MOMO");
        Repository repository = new Repository(payment);
        Methods methods = new Methods(payment.TransactionMethodId, invalidField == "provider" ? "OTHER" : "MOMO");
        ReceiveMomoIpn_UCInput input = CreateNotification(payment.Id, invalidField == "amount" ? 20000 : 10000);
        if (invalidField == "signature")
        {
            input = input with { Signature = new string('0', 64) };
        }

        await Assert.ThrowsAsync<BusinessException>(() => CreateHandler(repository, methods).Handle(input, default));
        Assert.Equal(TransactionStatus.PENDING, payment.Status);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Theory]
    [InlineData(9000)]
    [InlineData(1006)]
    public async Task NonSuccessDoesNotMarkPaymentAsPaid(int resultCode)
    {
        Payment payment = NewPayment();
        Repository repository = new Repository(payment);
        ReceiveMomoIpn_UCInput input = CreateNotification(payment.Id, resultCode: resultCode);

        Assert.False((await CreateHandler(repository).Handle(input, default)).Updated);
        Assert.Equal(TransactionStatus.PENDING, payment.Status);
    }

    [Fact]
    public void OnlineConfirmationRejectsDifferentTransactionAndCancelledPayment()
    {
        Payment payment = NewPayment();
        payment.ConfirmOnline("first");
        Assert.Throws<DomainException>(() => payment.ConfirmOnline("second"));

        Payment cancelled = NewPayment();
        cancelled.CancelForReplacement(TransactionMethodType.CASH);
        Assert.Throws<DomainException>(() => cancelled.ConfirmOnline("first"));
    }

    private static Payment NewPayment(string gatewayCode = "MOMO")
    {
        Guid methodId = Guid.NewGuid();
        TransactionMethod method = new TransactionMethod(methodId, "Online", TransactionMethodType.ONLINE, gatewayCode);
        return new Payment(PaymentType.ORDER, Guid.NewGuid(), methodId, 10000, method);
    }

    private static ReceiveMomoIpn_UCHandler CreateHandler(Repository repository, Methods? methods = null)
    {
        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(MomoCallbackController).Assembly));
        services.AddSingleton<IPaymentRepository>(repository);
        services.AddSingleton<ITransactionMethodRepository>(methods ?? new Methods(repository.Payment.TransactionMethodId, "MOMO"));
        services.AddSingleton<IExecuteAtomically, Transaction>();
        ServiceProvider provider = services.BuildServiceProvider();
        return new ReceiveMomoIpn_UCHandler(Options.Create(Settings),
            provider.GetRequiredService<ISender>());
    }

    private static ReceiveMomoIpn_UCInput CreateNotification(Guid id, long amount = 10000, int resultCode = 0)
    {
        string orderId = id.ToString("N");
        string raw = "accessKey=access&amount=" + amount.ToString(CultureInfo.InvariantCulture)
            + "&extraData=&message=Thành công&orderId=" + orderId
            + "&orderInfo=Thanh toán&orderType=momo_wallet&partnerCode=TEST&payType=qr"
            + "&requestId=request-1&responseTime=1700000000000&resultCode="
            + resultCode.ToString(CultureInfo.InvariantCulture) + "&transId=123456";
        string signature = Convert.ToHexStringLower(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("secret"), Encoding.UTF8.GetBytes(raw)));
        return new ReceiveMomoIpn_UCInput("TEST", orderId, "request-1", amount,
            "Thanh toán", "momo_wallet", 123456, resultCode, "Thành công", "qr",
            1700000000000, "", signature);
    }

    private sealed class Transaction : IExecuteAtomically
    {
        public Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> mainTask, CancellationToken cancellationToken = default)
        {
            return mainTask();
        }
    }

    private sealed class Methods : ITransactionMethodRepository
    {
        private readonly Guid _id;
        private readonly string _code;
        public Methods(Guid id, string code)
        {
            _id = id;
            _code = code;
        }
        public Task<TransactionMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<TransactionMethod?>(new TransactionMethod(_id, "MoMo", TransactionMethodType.ONLINE, _code));
        }
        public Task<List<TransactionMethod>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> AnyAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class Repository : IPaymentRepository
    {
        public Payment Payment { get; }
        public int UpdateCount { get; private set; }
        public Repository(Payment payment)
        {
            Payment = payment;
        }
        public Task<Payment?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Payment?>(id == Payment.Id ? Payment : null);
        }
        public Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }
        public Task LockReferenceAsync(PaymentType type, Guid refId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Payment?>(id == Payment.Id ? Payment : null);
        }
        public Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize, PaymentType? type, TransactionStatus? status, Guid? refId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<Payment>> GetByRefIdAsync(Guid refId, PaymentType type, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
