using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Controllers;

[Route("api/[controller]")]
public class CategoriesController : ODataController
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    /// <summary>GET all categories (Hỗ trợ OData Query: $filter, $select, $orderby, $top, $skip...)</summary>
    [HttpGet]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<CategoryDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IQueryable>> Get()
    {
        var categories = await _service.GetAllAsync();
        // Trả về IQueryable để OData tự động áp dụng query string từ client
        return Ok(categories.AsQueryable());
    }

    /// <summary>GET a category by ID using OData convention or route</summary>
    [HttpGet("{id:int}")]
    [EnableQuery]
    [ProducesResponseType(typeof(CategoryDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] short id)
    {
        var category = await _service.GetByIdAsync(id);
        if (category == null)
            return NotFound(new { message = $"Category with ID {id} not found." });

        return Ok(category);
    }

    /// <summary>GET only active categories (Có hỗ trợ OData Query)</summary>
    [HttpGet("active")]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<CategoryDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IQueryable>> GetActive()
    {
        var categories = await _service.GetActiveAsync();
        return Ok(categories.AsQueryable());
    }

    /// <summary>GET categories by search keyword (Có hỗ trợ OData Query)</summary>
    [HttpGet("search")]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<CategoryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IQueryable>> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "Keyword must not be empty." });

        var categories = await _service.SearchAsync(keyword);
        return Ok(categories.AsQueryable());
    }

    /// <summary>POST create a new category (Staff only)</summary>
    [HttpPost]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType(typeof(CategoryDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Post([FromBody] CategoryCreateDTO dto)
    {
        var created = await _service.CreateAsync(dto);
        Response.Headers["X-Created-Status"] = "Category created successfully";
        return CreatedAtAction(nameof(GetById), new { id = created.CategoryId }, created);
    }

    /// <summary>PUT update an existing category (Staff only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType(typeof(CategoryDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromRoute] short id, [FromBody] CategoryUpdateDTO dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null)
            return NotFound(new { message = $"Category with ID {id} not found." });

        return Ok(updated);
    }

    /// <summary>DELETE a category (Staff only) - fails if category has news articles</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] short id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = $"Category with ID {id} not found." });

        return NoContent();
    }
}
