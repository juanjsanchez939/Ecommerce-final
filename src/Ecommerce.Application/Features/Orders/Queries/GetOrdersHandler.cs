using Ecommerce.Application.Common.DTOs;
using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Features.Orders.Queries;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOrdersHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        var isAdmin = _currentUser.IsAdmin;

        var query = _context.Orders.AsQueryable();

        if (!isAdmin && !string.IsNullOrEmpty(currentUserId))
        {
            var userId = Guid.Parse(currentUserId);
            query = query.Where(o => o.UserId == userId);
        }

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                ItemCount = o.OrderItems.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
