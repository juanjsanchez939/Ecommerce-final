using MediatR;

namespace Ecommerce.Application.Features.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest;
