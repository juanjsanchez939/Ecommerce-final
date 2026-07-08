using MediatR;

namespace Ecommerce.Application.Features.Products.Commands;

public record DeleteProductCommand(Guid Id) : IRequest;
