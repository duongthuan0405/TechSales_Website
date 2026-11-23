using CommonModule.BusinessExceptions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using PromotionModule.Application.Exceptions;
using PromotionModule.Infrastructure.Persistence;
using PromotionModule.Infrastructure.Services;
using Xunit;

namespace PromotionModule.Tests;

public sealed class VoucherUniqueViolationTests
{
    [Theory]
    [InlineData("IX_Voucher_Code", true)]
    [InlineData("PK_Voucher", false)]
    [InlineData("IX_Other", false)]
    public async Task OnlyDuplicateVoucherCodeIsTranslated(string constraintName, bool expectedConflict)
    {
        PostgresException postgresError = new PostgresException(
            "duplicate value", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation,
            constraintName: constraintName);
        DbUpdateException writeError = new DbUpdateException("test storage error", postgresError);
        await using SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<PromotionDbContext> options = new DbContextOptionsBuilder<PromotionDbContext>()
            .UseSqlite(connection).AddInterceptors(new WriteFailure(writeError)).Options;
        await using PromotionDbContext db = new PromotionDbContext(options);
        ExecuteAtomically transaction = new ExecuteAtomically(db, NullLogger<ExecuteAtomically>.Instance);

        if (expectedConflict)
        {
            BusinessException error = await Assert.ThrowsAsync<BusinessException>(() =>
                transaction.ExecuteAtomicallyAsync(() => Task.FromResult(0)));
            Assert.Equal(BusinessExceptionCode.CONFLICT, error.Code);
            Assert.Equal(BusinessExceptionMessage.DuplicateCode, error.Message);
        }
        else
        {
            DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(() =>
                transaction.ExecuteAtomicallyAsync(() => Task.FromResult(0)));
            Assert.Same(writeError, error);
        }
        Assert.Null(db.Database.CurrentTransaction);
    }

    private sealed class WriteFailure : SaveChangesInterceptor
    {
        private readonly DbUpdateException _error;

        public WriteFailure(DbUpdateException error)
        {
            _error = error;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            throw _error;
        }
    }
}
