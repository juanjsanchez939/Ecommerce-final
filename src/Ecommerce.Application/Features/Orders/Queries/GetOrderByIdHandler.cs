using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.Orders.Queries;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOrderByIdHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<OrderDetailDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order is null)
            throw new NotFoundException(nameof(Order), request.Id);

        if (!_currentUser.IsAdmin && _currentUser.UserId is not null)
        {
            var currentUserId = Guid.Parse(_currentUser.UserId);
            if (order.UserId != currentUserId)
                throw new NotFoundException(nameof(Order), request.Id);
        }

        return new OrderDetailDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ShippingStreet = order.ShippingAddress.Street,
            ShippingCity = order.ShippingAddress.City,
            ShippingState = order.ShippingAddress.State,
            ShippingCountry = order.ShippingAddress.Country,
            ShippingZipCode = order.ShippingAddress.ZipCode,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                UnitPrice = oi.UnitPrice.Amount,
                Quantity = oi.Quantity,
                Subtotal = oi.Subtotal
            }).ToList()
        };
    }
}
