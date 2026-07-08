using FluentValidation;
using MediatR;

namespace Ecommerce.Application.Features.Categories.Commands;

public record CreateCategoryCommand : IRequest<Guid>
{
    public string Name { get; init; }
    public string Description { get; init; }
}

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
