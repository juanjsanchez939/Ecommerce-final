using Ecommerce.Application.Common.DTOs;
using Ecommerce.Application.Features.Cart.Commands;
using Ecommerce.Application.Features.Cart.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartDto>>> Get()
    {
        var result = await _mediator.Send(new GetCartQuery());
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<object>>> AddItem([FromBody] AddToCartCommand command)
    {
        await _mediator.Send(command);
        return Ok(ApiResponse<object>.Ok(null!, "Item added to cart."));
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveItem(Guid productId)
    {
        await _mediator.Send(new RemoveFromCartCommand(productId));
        return Ok(ApiResponse<object>.Ok(null!, "Item removed from cart."));
    }
}
