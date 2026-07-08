using MediatR;

namespace Ecommerce.Application.Features.Cart.Commands;

public record RemoveFromCartCommand(Guid ProductId) : IRequest;
