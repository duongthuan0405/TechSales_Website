namespace CartModule.Presentation.DTOs;

public sealed record AddCartItemRequest(Guid ProductId, int Quantity = 1);
