using System.Text.Json.Serialization;

namespace PromotionModule.Presentation.DTOs;

public sealed record VoucherRequest(
    [property: JsonRequired] string Code,
    [property: JsonRequired] string Type,
    [property: JsonRequired] long Value,
    int? MaxUsage, long? MinOrderAmount, DateTimeOffset? StartDate, DateTimeOffset? EndDate);
