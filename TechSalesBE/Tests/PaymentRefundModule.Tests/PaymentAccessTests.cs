using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using CommonModule.BusinessExceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentRefundModule.Application.Ports;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Application.UseCases.GetPaymentsForStaff;
using PaymentRefundModule.Application.UseCases.GetPayments;
using PaymentRefundModule.Application.UseCases.GetPaymentByRefId;
using PaymentRefundModule.Application.UseCases.CreatePayment;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Presentation.Controllers;
using Xunit;

namespace PaymentRefundModule.Tests;

public sealed class PaymentAccessTests
{
    [Theory]
    [InlineData("ORDER", true)]
    [InlineData("repair", true)]
    [InlineData(" order ", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("0", false)]
    [InlineData("1", false)]
    [InlineData("OTHER", false)]
    public void CreatePaymentAcceptsOnlyNamedStringTypes(string? type, bool expected)
    {
        CreatePayment_UCInput_Validator validator = new CreatePayment_UCInput_Validator();
        Assert.Equal(expected, validator.Validate(new CreatePayment_UCInput(Guid.NewGuid(), type,
            Guid.NewGuid(), Guid.NewGuid())).IsValid);
        if (expected)
        {
            Assert.True(Enum.IsDefined(PaymentInputParser.RequiredType(type)));
        }
    }

    [Fact]
    public async Task CustomerPaginationCountsOnlyOwnedMatchingPayments()
    {
        Guid ownedRef = Guid.NewGuid();
        Payment first = new Payment(PaymentType.ORDER, ownedRef, Guid.NewGuid(), 100_000);
        Payment second = new Payment(PaymentType.ORDER, ownedRef, Guid.NewGuid(), 100_000);
        Payment foreign = new Payment(PaymentType.ORDER, Guid.NewGuid(), Guid.NewGuid(), 100_000);
        Payment repair = new Payment(PaymentType.REPAIR, ownedRef, Guid.NewGuid(), 100_000);
        Repository repository = new Repository(new List<Payment> { foreign, first, repair, second });
        Ownership ownership = new Ownership(true, ownedRef);
        GetPayments_UCHandler handler = new GetPayments_UCHandler(repository, new PaymentOwnershipService(ownership, ownership));
        GetPayments_UCOutput output = await handler.Handle(new GetPayments_UCInput(Guid.NewGuid(),
            2, 1, "ORDER", "PENDING"), CancellationToken.None);
        Assert.Equal(2, output.TotalCount);
        Assert.Equal(2, output.PageNumber);
        Assert.Single(output.Items);
        Assert.Equal(first.Id, output.Items[0].Id);
        GetPayments_UCOutput empty = await handler.Handle(new GetPayments_UCInput(Guid.NewGuid(),
            3, 1, "ORDER", "PENDING"), CancellationToken.None);
        Assert.Empty(empty.Items);
        Assert.Equal(2, empty.TotalCount);
        GetPayments_UCOutput noMatch = await handler.Handle(new GetPayments_UCInput(Guid.NewGuid(),
            1, 20, "ORDER", "FAILED"), CancellationToken.None);
        Assert.Empty(noMatch.Items);
        Assert.Equal(0, noMatch.TotalCount);
    }

    [Fact]
    public void CustomerListValidatorsRejectInvalidFiltersAndPages()
    {
        GetPayments_UCInput_Validator list = new GetPayments_UCInput_Validator();
        Assert.False(list.Validate(new GetPayments_UCInput(Guid.NewGuid(), 0)).IsValid);
        Assert.False(list.Validate(new GetPayments_UCInput(Guid.NewGuid(), 1, 101)).IsValid);
        Assert.False(list.Validate(new GetPayments_UCInput(Guid.NewGuid(), 1, 20, "99")).IsValid);
        Assert.False(list.Validate(new GetPayments_UCInput(Guid.NewGuid(), 1, 20, null, "99")).IsValid);
        Assert.False(list.Validate(new GetPayments_UCInput(Guid.NewGuid(), RefId: Guid.Empty)).IsValid);
        GetPaymentByRefId_UCInput_Validator byRef = new GetPaymentByRefId_UCInput_Validator();
        Assert.False(byRef.Validate(new GetPaymentByRefId_UCInput(Guid.NewGuid(), Guid.NewGuid(), null)).IsValid);
        Assert.False(byRef.Validate(new GetPaymentByRefId_UCInput(Guid.NewGuid(), Guid.NewGuid(), "ORDER", PageSize: 101)).IsValid);
        Assert.True(byRef.Validate(new GetPaymentByRefId_UCInput(Guid.NewGuid(), Guid.NewGuid(), "ORDER", Status: "PENDING")).IsValid);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public void StaffListRejectsInvalidPagination(int pageNumber, int pageSize)
    {
        GetPaymentsForStaff_UCInput_Validator validator = new GetPaymentsForStaff_UCInput_Validator();
        Assert.False(validator.Validate(new GetPaymentsForStaff_UCInput(pageNumber, pageSize, null, null, null)).IsValid);
    }

    [Fact]
    public void StaffListValidatesFilters()
    {
        GetPaymentsForStaff_UCInput_Validator validator = new GetPaymentsForStaff_UCInput_Validator();
        Assert.False(validator.Validate(new GetPaymentsForStaff_UCInput(1, 20, "99", null, null)).IsValid);
        Assert.False(validator.Validate(new GetPaymentsForStaff_UCInput(1, 20, null, "99", null)).IsValid);
        Assert.False(validator.Validate(new GetPaymentsForStaff_UCInput(1, 20, null, null, Guid.Empty)).IsValid);
        Assert.True(validator.Validate(new GetPaymentsForStaff_UCInput(1, 20, "ORDER", "PENDING", Guid.NewGuid())).IsValid);
    }

    [Theory]
    [InlineData("Staff")]
    [InlineData("Business Admin")]
    [InlineData("Technical Admin")]
    public async Task StaffCanReadAnotherCustomersPayment(string role)
    {
        using TestServer server = CreateServer();
        using HttpClient client = server.CreateClient();
        client.DefaultRequestHeaders.Add("TestRole", role);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/payments/{Repository.Payment.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/payments/by-ref/{Repository.Payment.OrderId}?type=ORDER")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/admin/payments")).StatusCode);
    }

    [Fact]
    public async Task CustomerCannotBypassOwnerCheckOrReadStaffList()
    {
        using TestServer server = CreateServer();
        using HttpClient client = server.CreateClient();
        client.DefaultRequestHeaders.Add("TestRole", "Customer");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/payments/{Repository.Payment.Id}?canViewAll=true")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/payments/by-ref/{Repository.Payment.OrderId}?type=ORDER&canViewAll=true")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/payments")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/api/admin/payments/{Repository.Payment.Id}/confirm-cash", null)).StatusCode);
        string list = await client.GetStringAsync("/api/payments");
        Assert.DoesNotContain(Repository.Payment.Id.ToString(), list);
    }

    [Fact]
    public async Task OwnerCanReadPayment()
    {
        using TestServer server = CreateServer(true);
        using HttpClient client = server.CreateClient();
        client.DefaultRequestHeaders.Add("TestRole", "Customer");
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/payments/{Repository.Payment.Id}")).StatusCode);
        Assert.Contains(Repository.Payment.Id.ToString(), await client.GetStringAsync("/api/payments"));
    }

    [Fact]
    public async Task AnonymousCannotReadPayments()
    {
        using TestServer server = CreateServer();
        using HttpClient client = server.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/payments")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync($"/api/admin/payments/{Repository.Payment.Id}/confirm-cash", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/api/payments/{Repository.Payment.Id}")).StatusCode);
    }

    private static TestServer CreateServer(bool isOwner = false)
    {
        return new TestServer(new WebHostBuilder().ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthentication>("Test", options => { });
            services.AddAuthorization();
            services.AddControllers().AddApplicationPart(typeof(PaymentController).Assembly);
            services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(PaymentController).Assembly));
            services.AddSingleton<IPaymentRepository, Repository>();
            services.AddSingleton<IOrderOwnershipPort>(new Ownership(isOwner));
            services.AddSingleton<IRepairOwnershipPort>(new Ownership(isOwner));
            services.AddScoped<PaymentOwnershipService>();
        }).Configure(app =>
        {
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (BusinessException)
                {
                    context.Response.StatusCode = 403;
                }
            });
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }));
    }

    private sealed class TestAuthentication : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string role = Request.Headers["TestRole"].ToString();
            if (string.IsNullOrEmpty(role))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            ClaimsIdentity identity = new ClaimsIdentity(new[]
            {
                new Claim("sub", "8d01ec19-1174-4915-a82e-ff226f80c8e0"),
                new Claim(ClaimTypes.Role, role)
            }, "Test");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Test")));
        }
    }

    private sealed class Ownership : IOrderOwnershipPort, IRepairOwnershipPort
    {
        private readonly bool _isOwner;
        private readonly Guid? _ownedRef;
        public Ownership(bool isOwner, Guid? ownedRef = null)
        {
            _isOwner = isOwner;
            _ownedRef = ownedRef;
        }
        public Task<bool> IsOwnedByCustomerAsync(Guid refId, Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_isOwner && (!_ownedRef.HasValue || _ownedRef.Value == refId));
        }
    }

    private sealed class Repository : IPaymentRepository
    {
        internal static readonly Payment Payment = new Payment(PaymentType.ORDER, Guid.NewGuid(), Guid.NewGuid(), 100_000);
        private readonly List<Payment> _items;
        public Repository() : this(new List<Payment> { Payment })
        {
        }
        public Repository(List<Payment> items)
        {
            _items = items;
        }
        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Payment?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Payment?>(id == Payment.Id ? Payment : null);
        public Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(_items);
        public Task<List<Payment>> GetByRefIdAsync(Guid refId, PaymentType type, CancellationToken cancellationToken = default) => GetAllAsync(cancellationToken);
        public Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize, PaymentType? type,
            TransactionStatus? status, Guid? refId, CancellationToken cancellationToken = default)
            => Task.FromResult((new List<Payment> { Payment }, 1));
    }
}
