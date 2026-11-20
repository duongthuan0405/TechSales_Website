using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Application.UseCases.ReceiveVnpayIpn;
using PaymentRefundModule.Presentation.Controllers;
using Xunit;

namespace PaymentRefundModule.Tests;

public sealed class VnpayIpnTests
{
    private static readonly VnpayOptions Settings = new VnpayOptions { TmnCode = "TESTCODE", HashSecret = "test-secret" };

    [Fact]
    public async Task AnonymousIpnConfirmsExactlyOnceAndUsesExactProtocolPropertyNames()
    {
        Repository repository = new Repository(NewPayment());
        using TestServer server = CreateServer(repository);
        using HttpClient client = server.CreateClient();
        string url = CallbackUrl(Fields(repository.Payment));

        Assert.Equal("00", await GetCode(client, url));
        Assert.Equal("02", await GetCode(client, url));
        Assert.Equal(TransactionStatus.SUCCESS, repository.Payment.Status);
        Assert.Equal("123456", repository.Payment.TransactionRef);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Theory]
    [InlineData("signature", "97")]
    [InlineData("nonhex", "97")]
    [InlineData("tamper", "97")]
    [InlineData("merchant", "97")]
    [InlineData("unknown-payment", "01")]
    [InlineData("malformed-reference", "01")]
    [InlineData("amount", "04")]
    [InlineData("fractional-vnd", "04")]
    [InlineData("overflow", "04")]
    [InlineData("missing-status", "99")]
    [InlineData("pending", "99")]
    [InlineData("conflicting-status", "99")]
    [InlineData("zero-transaction", "99")]
    public async Task InvalidCallbacksDoNotChangePayment(string scenario, string expectedCode)
    {
        Repository repository = new Repository(NewPayment());
        Dictionary<string, string> fields = Fields(repository.Payment);
        switch (scenario)
        {
            case "merchant": fields["vnp_TmnCode"] = "OTHER"; break;
            case "unknown-payment": fields["vnp_TxnRef"] = Guid.NewGuid().ToString("N"); break;
            case "malformed-reference": fields["vnp_TxnRef"] = "not-a-guid"; break;
            case "amount": fields["vnp_Amount"] = "2000000"; break;
            case "fractional-vnd": fields["vnp_Amount"] = "1000001"; break;
            case "overflow": fields["vnp_Amount"] = "999999999999999999999999"; break;
            case "missing-status": fields.Remove("vnp_TransactionStatus"); break;
            case "pending": fields["vnp_ResponseCode"] = "99"; fields["vnp_TransactionStatus"] = "01"; break;
            case "conflicting-status": fields["vnp_TransactionStatus"] = "02"; break;
            case "zero-transaction": fields["vnp_TransactionNo"] = "0"; break;
        }
        fields["vnp_SecureHash"] = Sign(fields);
        if (scenario == "signature")
        {
            fields["vnp_SecureHash"] = new string('0', 128);
        }
        if (scenario == "nonhex")
        {
            fields["vnp_SecureHash"] = new string('z', 128);
        }
        if (scenario == "tamper")
        {
            fields["vnp_Amount"] = "2000000";
        }
        using TestServer server = CreateServer(repository);
        using HttpClient client = server.CreateClient();

        Assert.Equal(expectedCode, await GetCode(client, CallbackUrl(fields, sign: false)));
        Assert.Equal(TransactionStatus.PENDING, repository.Payment.Status);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task RejectsDuplicateQueryParameters()
    {
        Repository repository = new Repository(NewPayment());
        using TestServer server = CreateServer(repository);
        using HttpClient client = server.CreateClient();
        string url = CallbackUrl(Fields(repository.Payment)) + "&vnp_Amount=1000000";
        Assert.Equal("97", await GetCode(client, url));
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task RejectsOtherGatewayPaymentEvenWithValidSignature()
    {
        Repository repository = new Repository(NewPayment("MOMO"));
        using TestServer server = CreateServer(repository);
        using HttpClient client = server.CreateClient();
        Assert.Equal("01", await GetCode(client, CallbackUrl(Fields(repository.Payment))));
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task CancelledPaymentIsFailedAndDuplicateIsAcknowledged()
    {
        Repository repository = new Repository(NewPayment());
        Dictionary<string, string> fields = Fields(repository.Payment);
        fields["vnp_ResponseCode"] = "24";
        fields["vnp_TransactionStatus"] = "02";
        fields["vnp_TransactionNo"] = "0";
        using TestServer server = CreateServer(repository);
        using HttpClient client = server.CreateClient();
        string url = CallbackUrl(fields);

        Assert.Equal("00", await GetCode(client, url));
        Assert.Equal("02", await GetCode(client, url));
        Assert.Equal(TransactionStatus.FAILED, repository.Payment.Status);
        Assert.NotNull(repository.Payment.CompletedAt);
        Assert.False(string.IsNullOrEmpty(repository.Payment.FailureReason));
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task FailureAndConflictingTransactionCannotOverwriteSuccess()
    {
        Repository repository = new Repository(NewPayment());
        using TestServer server = CreateServer(repository);
        using HttpClient client = server.CreateClient();
        Dictionary<string, string> fields = Fields(repository.Payment);
        Assert.Equal("00", await GetCode(client, CallbackUrl(fields)));

        fields["vnp_ResponseCode"] = "24";
        fields["vnp_TransactionStatus"] = "02";
        Assert.Equal("99", await GetCode(client, CallbackUrl(fields)));
        fields["vnp_ResponseCode"] = "00";
        fields["vnp_TransactionStatus"] = "00";
        fields["vnp_TransactionNo"] = "999999";
        Assert.Equal("99", await GetCode(client, CallbackUrl(fields)));
        Assert.Equal(TransactionStatus.SUCCESS, repository.Payment.Status);
        Assert.Equal("123456", repository.Payment.TransactionRef);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task DatabaseErrorReturnsRetryableProtocolResponse()
    {
        Repository repository = new Repository(NewPayment()) { FailReads = true };
        using TestServer server = CreateServer(repository);
        using HttpClient client = server.CreateClient();
        Assert.Equal("99", await GetCode(client, CallbackUrl(Fields(repository.Payment))));
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public void VerifierAcceptsEncodedSpecialCharactersAndExcludesHashMetadata()
    {
        Dictionary<string, string> fields = Fields(NewPayment());
        fields["vnp_OrderInfo"] = "Thanh toan + & = / Tiếng Việt";
        fields["vnp_SecureHashType"] = "HmacSHA512";
        fields["vnp_BankTranNo"] = "";
        fields["vnp_SecureHash"] = Sign(fields).ToUpperInvariant();
        ReceiveVnpayIpn_UCHandler verifier = new ReceiveVnpayIpn_UCHandler(Options.Create(Settings), null!, null!);
        Assert.True(verifier.IsValid(fields));
        fields["vnp_OrderInfo"] += " tampered";
        Assert.False(verifier.IsValid(fields));
        Assert.False(new ReceiveVnpayIpn_UCHandler(Options.Create(new VnpayOptions()), null!, null!).IsValid(fields));
    }

    private static Payment NewPayment(string gateway = "VNPAY")
    {
        Guid methodId = Guid.NewGuid();
        TransactionMethod method = new TransactionMethod(methodId, gateway, TransactionMethodType.ONLINE, gateway);
        return new Payment(PaymentType.ORDER, Guid.NewGuid(), methodId, 10000, method);
    }

    private static Dictionary<string, string> Fields(Payment payment)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_TmnCode"] = Settings.TmnCode,
            ["vnp_TxnRef"] = payment.Id.ToString("N"),
            ["vnp_Amount"] = "1000000",
            ["vnp_OrderInfo"] = "Thanh toan giao dich",
            ["vnp_BankCode"] = "NCB",
            ["vnp_ResponseCode"] = "00",
            ["vnp_TransactionStatus"] = "00",
            ["vnp_TransactionNo"] = "123456"
        };
    }

    private static string Sign(Dictionary<string, string> fields)
    {
        string data = string.Join("&", fields.Where(field => field.Key != "vnp_SecureHash"
                && field.Key != "vnp_SecureHashType" && !string.IsNullOrEmpty(field.Value))
            .OrderBy(field => field.Key, StringComparer.Ordinal)
            .Select(field => WebUtility.UrlEncode(field.Key) + "=" + WebUtility.UrlEncode(field.Value)));
        return Convert.ToHexStringLower(HMACSHA512.HashData(
            Encoding.UTF8.GetBytes(Settings.HashSecret), Encoding.UTF8.GetBytes(data)));
    }

    private static string CallbackUrl(Dictionary<string, string> fields, bool sign = true)
    {
        if (sign)
        {
            fields["vnp_SecureHash"] = Sign(fields);
        }
        return QueryHelpers.AddQueryString("/api/payments/vnpay/ipn", fields!);
    }

    private static async Task<string> GetCode(HttpClient client, string url)
    {
        using HttpResponseMessage response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument body = await response.Content.ReadFromJsonAsync<JsonDocument>() ?? throw new InvalidOperationException();
        Assert.True(body.RootElement.TryGetProperty("Message", out _));
        Assert.False(body.RootElement.TryGetProperty("success", out _));
        return body.RootElement.GetProperty("RspCode").GetString()!;
    }

    private static TestServer CreateServer(Repository repository)
    {
        return new TestServer(new WebHostBuilder().ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddControllers().AddApplicationPart(typeof(VnpayCallbackController).Assembly);
            services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(VnpayCallbackController).Assembly));
            services.AddSingleton(Options.Create(Settings));
            services.AddSingleton<IPaymentRepository>(repository);
            services.AddSingleton<IExecuteAtomically, Transaction>();
        }).Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }));
    }

    private sealed class Transaction : IExecuteAtomically
    {
        public Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> mainTask, CancellationToken cancellationToken = default)
        {
            return mainTask();
        }
    }

    private sealed class Repository : IPaymentRepository
    {
        public Payment Payment { get; }
        public int UpdateCount { get; private set; }
        public bool FailReads { get; init; }
        public Repository(Payment payment)
        {
            Payment = payment;
        }
        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (FailReads)
            {
                throw new InvalidOperationException("Database unavailable");
            }
            return Task.FromResult<Payment?>(id == Payment.Id ? Payment : null);
        }
        public Task<Payment?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return GetByIdAsync(id, cancellationToken);
        }
        public Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }
        public Task LockReferenceAsync(PaymentType type, Guid refId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize, PaymentType? type, TransactionStatus? status, Guid? refId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<Payment>> GetByRefIdAsync(Guid refId, PaymentType type, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
