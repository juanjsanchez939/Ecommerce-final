using FluentValidation;
using MediatR;

namespace Ecommerce.Application.Features.Cart.Commands;

public record AddToCartCommand : IRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; } = 1;
}

public class AddToCartValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
