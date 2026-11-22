using CartModule.Application.UseCases.Carts.AddCartItem;
using CartModule.Application.UseCases.Carts.GetCart;
using CartModule.Application.UseCases.Carts.RemoveCartItem;
using CartModule.Application.UseCases.Carts.UpdateCartItem;
using FluentValidation.Results;
using Xunit;

namespace CartModule.Tests;

public sealed class CartValidationTests
{
    private const string InvalidUser = "A non-empty user ID is required to access the cart.";
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");
    private static readonly Guid ProductId = Guid.Parse("11111111-1111-4111-8111-111111111111");

    [Fact(DisplayName = "CART-UNIT-01 All validators reject an empty actor ID")]
    public void RejectsAnEmptyActorForEachUseCase()
    {
        ValidationResult[] results =
        [
            new GetCart_UCInput_Validator().Validate(new GetCart_UCInput(Guid.Empty)),
            new AddCartItem_UCInput_Validator().Validate(new AddCartItem_UCInput(Guid.Empty, ProductId, 1)),
            new UpdateCartItem_UCInput_Validator().Validate(new UpdateCartItem_UCInput(Guid.Empty, ProductId, 1)),
            new RemoveCartItem_UCInput_Validator().Validate(new RemoveCartItem_UCInput(Guid.Empty, ProductId))
        ];
        foreach (ValidationResult result in results)
        {
            ValidationFailure failure = Assert.Single(result.Errors);
            Assert.Equal("UserId", failure.PropertyName);
            Assert.Equal(InvalidUser, failure.ErrorMessage);
        }
    }

    [Theory(DisplayName = "CART-UNIT-02 Quantity partitions and Int32 boundaries")]
    [InlineData(int.MinValue, false)]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(int.MaxValue - 1, true)]
    [InlineData(int.MaxValue, true)]
    public void ValidatesQuantityBoundariesForAddAndUpdate(int quantity, bool valid)
    {
        ValidationResult add = new AddCartItem_UCInput_Validator().Validate(new AddCartItem_UCInput(UserId, ProductId, quantity));
        ValidationResult update = new UpdateCartItem_UCInput_Validator().Validate(new UpdateCartItem_UCInput(UserId, ProductId, quantity));
        if (valid)
        {
            Assert.True(add.IsValid);
            Assert.True(update.IsValid);
        }
        else
        {
            ValidationFailure addFailure = Assert.Single(add.Errors);
            Assert.Equal("Quantity", addFailure.PropertyName);
            Assert.Equal("Quantity to add must be a positive integer.", addFailure.ErrorMessage);
            ValidationFailure updateFailure = Assert.Single(update.Errors);
            Assert.Equal("Quantity", updateFailure.PropertyName);
            Assert.Equal("The new quantity must be a positive integer.", updateFailure.ErrorMessage);
        }
    }
}
