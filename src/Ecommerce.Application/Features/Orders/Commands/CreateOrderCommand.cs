using Ecommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Commands;

public record CreateOrderCommand : IRequest<Guid>
{
    public string Street { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string Country { get; init; }
    public string ZipCode { get; init; }
}

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(20);
    }
}
