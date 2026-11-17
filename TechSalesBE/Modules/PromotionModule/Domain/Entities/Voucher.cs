using CommonModule.Domain.Exceptions;
using PromotionModule.Domain.Enums;
using PromotionModule.Domain.Exceptions;
using PromotionModule.Domain.Rules;

namespace PromotionModule.Domain.Entities;

internal sealed class Voucher
{
    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public VoucherType Type { get; private set; }
    public long? Value { get; private set; }
    public int? MaxUsage { get; private set; }
    public int UsedCount { get; private set; }
    public long? MinOrderAmount { get; private set; }
    public DateTimeOffset? StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public Voucher(Guid id,
        string code,
        VoucherType type,
        long? value,
        int? maxUsage,
        int usedCount,
        long? minOrderAmount,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        bool isActive,
        DateTimeOffset? createdAt,
        DateTimeOffset? updatedAt)
    {
        Id = id;
        Code = code;
        Type = type;
        Value = value;
        MaxUsage = maxUsage;
        UsedCount = usedCount;
        MinOrderAmount = minOrderAmount;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Voucher Create(string code, string type, long value, int? maxUsage,
        long? minOrderAmount, DateTimeOffset? startDate, DateTimeOffset? endDate, DateTimeOffset now)
    {
        bool isValid = VoucherRules.ValidDefinition(code, type, value, maxUsage, minOrderAmount, startDate, endDate);
        if (!isValid)
        {
            throw new DomainException(DomainExceptionCode.INVALID_VOUCHER, DomainExceptionMessage.InvalidVoucher);
        }

        return new Voucher(Guid.NewGuid(), VoucherRules.NormalizeCode(code), VoucherRules.ParseType(type),
            value, maxUsage, 0, minOrderAmount, startDate?.ToUniversalTime(), endDate?.ToUniversalTime(), true, now, now);
    }

    public void Update(string code, string type, long value, int? maxUsage, long? minOrderAmount,
        DateTimeOffset? startDate, DateTimeOffset? endDate, DateTimeOffset now)
    {
        bool isValid = VoucherRules.ValidDefinition(code, type, value, maxUsage, minOrderAmount, startDate, endDate);
        if (!isValid || !VoucherRules.CoversUsedCount(maxUsage, UsedCount))
        {
            throw new DomainException(DomainExceptionCode.INVALID_VOUCHER, DomainExceptionMessage.InvalidVoucher);
        }

        Code = VoucherRules.NormalizeCode(code);
        Type = VoucherRules.ParseType(type);
        Value = value;
        MaxUsage = maxUsage;
        MinOrderAmount = minOrderAmount;
        StartDate = startDate?.ToUniversalTime();
        EndDate = endDate?.ToUniversalTime();
        UpdatedAt = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        UpdatedAt = now;
    }
}
