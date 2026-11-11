using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Controllers;

[Route("api/[controller]")]
public class NewsArticlesController : ODataController
{
    private readonly INewsArticleService _service;

    public NewsArticlesController(INewsArticleService service)
    {
        _service = service;
    }

    /// <summary>GET all news articles (Hỗ trợ OData Query: $filter, $select, $orderby, $top, $skip...)</summary>
    [HttpGet]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<NewsArticleDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IQueryable>> Get()
    {
        var articles = await _service.GetAllAsync();
        return Ok(articles.AsQueryable());
    }

    /// <summary>GET active news articles (Hỗ trợ OData Query)</summary>
    [HttpGet("active")]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<NewsArticleDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IQueryable>> GetActive()
    {
        var articles = await _service.GetActiveAsync();
        return Ok(articles.AsQueryable());
    }

    /// <summary>GET a news article by ID using OData string route</summary>
    [HttpGet("{id}")]
    [EnableQuery]
    [ProducesResponseType(typeof(NewsArticleDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var article = await _service.GetByIdAsync(id);
        if (article == null)
            return NotFound(new { message = $"News article with ID '{id}' not found." });

        return Ok(article);
    }

    /// <summary>GET news articles by search keyword (Hỗ trợ OData Query)</summary>
    [HttpGet("search")]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<NewsArticleDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IQueryable>> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "Keyword must not be empty." });

        var articles = await _service.SearchAsync(keyword);
        return Ok(articles.AsQueryable());
    }

    /// <summary>GET news articles created by a specific staff account (Hỗ trợ OData Query)</summary>
    [HttpGet("by-creator/{createdById:int}")]
    [Authorize(Roles = "Staff")]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<NewsArticleDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IQueryable>> GetByCreator([FromRoute] short createdById)
    {
        var articles = await _service.GetByCreatedByAsync(createdById);
        return Ok(articles.AsQueryable());
    }

    /// <summary>GET news statistics report by date range (Hỗ trợ OData Query)</summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<NewsStatisticDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IQueryable>> GetStatistics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
            return BadRequest(new { message = "Start date must be before or equal to end date." });

        var stats = await _service.GetStatisticsAsync(startDate, endDate);
        return Ok(stats.AsQueryable());
    }

    /// <summary>POST create a new news article (Staff only)</summary>
    [HttpPost]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType(typeof(NewsArticleDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post([FromBody] NewsArticleCreateDTO dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.NewsArticleId }, created);
    }

    /// <summary>PUT update an existing news article (Staff only)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType(typeof(NewsArticleDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromRoute] string id, [FromBody] NewsArticleUpdateDTO dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null)
            return NotFound(new { message = $"News article with ID '{id}' not found." });

        return Ok(updated); // Trả về Status 200 chuẩn MVC cho hàm Update
    }

    /// <summary>DELETE a news article (Staff only)</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = $"News article with ID '{id}' not found." });

        return NoContent();
    }
}
