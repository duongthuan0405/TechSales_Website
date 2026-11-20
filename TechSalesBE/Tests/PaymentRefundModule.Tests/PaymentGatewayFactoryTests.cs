using CommonModule.BusinessExceptions;
using Microsoft.Extensions.Options;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Infrastructure.Payments;
using PaymentRefundModule.Infrastructure.Payments.Momo;
using PaymentRefundModule.Infrastructure.Payments.Vnpay;
using Xunit;

namespace PaymentRefundModule.Tests;

public sealed class PaymentGatewayFactoryTests
{
    [Theory]
    [InlineData("VNPAY")]
    [InlineData(" vnpay ")]
    public async Task ResolvesVnpayAlongsideMomo(string code)
    {
        Guid methodId = Guid.NewGuid();
        VnpayPaymentStrategy strategy = new VnpayPaymentStrategy(Options.Create(new VnpayOptions()));
        Methods methods = new Methods(new TransactionMethod(methodId, "VNPAY", TransactionMethodType.ONLINE, code));
        PaymentGatewayFactory factory = new PaymentGatewayFactory(methods,
            new List<IPaymentGatewayStrategy> { CreateMomoStrategy(), strategy });

        Assert.Same(strategy, await factory.GetStrategyAsync(methodId));
    }

    [Theory]
    [InlineData("MOMO")]
    [InlineData("Momo")]
    [InlineData(" momo ")]
    public async Task ResolvesStrategyFromMethodCode(string code)
    {
        MomoPaymentStrategy strategy = CreateMomoStrategy();
        Guid methodId = Guid.NewGuid();
        Methods methods = new Methods(new TransactionMethod(methodId, "MoMo", TransactionMethodType.ONLINE, code));
        PaymentGatewayFactory factory = new PaymentGatewayFactory(methods, new List<IPaymentGatewayStrategy> { strategy });

        Assert.Same(strategy, await factory.GetStrategyAsync(methodId));
        Assert.Equal(methodId, methods.RequestedId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("UNKNOWN")]
    public async Task RejectsMissingOrUnsupportedCode(string? code)
    {
        Guid methodId = Guid.NewGuid();
        Methods methods = new Methods(new TransactionMethod(methodId, "MoMo", TransactionMethodType.ONLINE, code));
        PaymentGatewayFactory factory = new PaymentGatewayFactory(methods, new List<IPaymentGatewayStrategy> { CreateMomoStrategy() });

        await Assert.ThrowsAsync<BusinessException>(() => factory.GetStrategyAsync(methodId));
    }

    [Fact]
    public async Task RejectsUnknownMethod()
    {
        PaymentGatewayFactory factory = new PaymentGatewayFactory(new Methods(null), new List<IPaymentGatewayStrategy> { CreateMomoStrategy() });

        BusinessException exception = await Assert.ThrowsAsync<BusinessException>(() => factory.GetStrategyAsync(Guid.NewGuid()));
        Assert.Equal(BusinessExceptionCode.NOT_FOUND, exception.Code);
    }

    private static MomoPaymentStrategy CreateMomoStrategy()
    {
        return new MomoPaymentStrategy(
            new HttpClient(),
            Options.Create(new MomoOptions()),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<MomoPaymentStrategy>.Instance);
    }

    private sealed class Methods : ITransactionMethodRepository
    {
        private readonly TransactionMethod? _method;
        public Guid RequestedId { get; private set; }

        public Methods(TransactionMethod? method)
        {
            _method = method;
        }

        public Task<TransactionMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            RequestedId = id;
            return Task.FromResult(_method);
        }

        public Task<List<TransactionMethod>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> AnyAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
