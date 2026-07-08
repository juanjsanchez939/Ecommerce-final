using Ecommerce.Application.Features.Categories.Commands;
using Ecommerce.Application.Features.Categories.Queries;
using Ecommerce.Application.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> Get()
    {
        var result = await _mediator.Send(new GetCategoriesQuery());
        return Ok(ApiResponse<List<CategoryDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<Guid>.Ok(result, "Category created successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        if (id != command.Id)
            return BadRequest(ApiResponse<object>.Fail("Id mismatch."));

        await _mediator.Send(command);
        return Ok(ApiResponse<object>.Ok(null!, "Category updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));
        return Ok(ApiResponse<object>.Ok(null!, "Category deleted successfully."));
    }
}
