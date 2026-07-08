using MediatR;

namespace Ecommerce.Application.Features.Orders.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDetailDto>;
