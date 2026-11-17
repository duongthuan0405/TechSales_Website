using System.Text.Json.Serialization;

namespace ShippingModule.Presentation.DTOs;

public sealed record SetShippingFeeRequest([property: JsonRequired] long Fee);
