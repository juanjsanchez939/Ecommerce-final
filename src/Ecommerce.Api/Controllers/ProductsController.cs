using Ecommerce.Application.Common.DTOs;
using Ecommerce.Application.Features.Products.Commands;
using Ecommerce.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetProductsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            IsActive = isActive
        };

        var result = await _mediator.Send(query);
        return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        return Ok(ApiResponse<ProductDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result },
            ApiResponse<Guid>.Ok(result, "Product created successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
            return BadRequest(ApiResponse<object>.Fail("Id mismatch."));

        await _mediator.Send(command);
        return Ok(ApiResponse<object>.Ok(null!, "Product updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return Ok(ApiResponse<object>.Ok(null!, "Product deactivated successfully."));
    }
}
