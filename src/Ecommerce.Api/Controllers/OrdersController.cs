using Ecommerce.Application.Common.DTOs;
using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Features.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetOrdersQuery { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _mediator.Send(query);
        return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));
        return Ok(ApiResponse<OrderDetailDto>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result },
            ApiResponse<Guid>.Ok(result, "Order created successfully."));
    }
}
