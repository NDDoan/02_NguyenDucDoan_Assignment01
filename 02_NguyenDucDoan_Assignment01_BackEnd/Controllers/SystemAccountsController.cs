using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Controllers;

[Route("api/[controller]")]
[Authorize]
public class SystemAccountsController : ODataController
{
    private readonly ISystemAccountService _service;

    public SystemAccountsController(ISystemAccountService service)
    {
        _service = service;
    }

    /// <summary>GET all system accounts (Admin only - Hỗ trợ OData Query: $filter, $select, $orderby, $top, $skip...)</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [EnableQuery]
    public async Task<ActionResult<IQueryable>> Get()
    {
        var accounts = await _service.GetAllAsync();
        return Ok(accounts.AsQueryable());
    }

    /// <summary>GET system accounts by search keyword (Admin only)</summary>
    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    [EnableQuery]
    public async Task<ActionResult<IQueryable>> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "Keyword must not be empty." });

        var accounts = await _service.SearchAsync(keyword);
        return Ok(accounts.AsQueryable());
    }

    /// <summary>GET a system account by ID (Admin or own profile)</summary>
    [HttpGet("({id:int})")]
    [Authorize(Roles = "Admin,Staff,Lecturer")]
    [EnableQuery]
    public async Task<IActionResult> GetById([FromRoute] short id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && currentUserId != id.ToString())
        {
            return Forbid();
        }

        var account = await _service.GetByIdAsync(id);
        if (account == null)
            return NotFound(new { message = $"Account with ID {id} not found." });

        return Ok(account);
    }

    /// <summary>POST create a new system account (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] SystemAccountCreateDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = await _service.CreateAsync(dto);
            return StatusCode(201, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>PUT update an existing system account (Admin or own profile)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Staff,Lecturer")]
    public async Task<IActionResult> Put([FromRoute] short id, [FromBody] SystemAccountUpdateDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin)
        {
            if (currentUserId != id.ToString())
                return Forbid();

            // Prevent non-admin from modifying their role
            var existing = await _service.GetByIdAsync(id);
            if (existing != null && existing.AccountRole.HasValue)
            {
                dto.AccountRole = existing.AccountRole.Value;
            }
        }

        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = $"Account with ID {id} not found." });

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>DELETE a system account (Admin only) - fails if account has news articles</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] short id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = $"Account with ID {id} not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>POST login with email and password</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _service.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(result);
    }
}