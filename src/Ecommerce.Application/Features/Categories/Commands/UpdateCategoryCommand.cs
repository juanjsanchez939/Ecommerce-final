using FluentValidation;
using MediatR;

namespace Ecommerce.Application.Features.Categories.Commands;

public record UpdateCategoryCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
