using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.Cart.Queries;

public class GetCartHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCartHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
            return new CartDto();

        var userId = Guid.Parse(_currentUser.UserId);

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null)
            return new CartDto();

        return new CartDto
        {
            Id = cart.Id,
            Items = cart.CartItems.Select(ci => new CartItemDto
            {
                ProductId = ci.ProductId,
                ProductName = ci.ProductName,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                Currency = ci.Currency,
                Subtotal = ci.Subtotal
            }).ToList(),
            Total = cart.Total,
            ItemCount = cart.CartItems.Count
        };
    }
}
