using MediatR;

namespace Ecommerce.Application.Features.Cart.Queries;

public record GetCartQuery : IRequest<CartDto>;

public class CartDto
{
    public Guid Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}

public class CartItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; }
    public decimal Subtotal { get; set; }
}
