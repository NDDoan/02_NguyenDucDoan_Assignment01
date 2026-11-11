using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
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
    [ProducesResponseType(typeof(IEnumerable<SystemAccountDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IQueryable>> Get()
    {
        var accounts = await _service.GetAllAsync();
        return Ok(accounts.AsQueryable());
    }

    /// <summary>GET system accounts by search keyword (Admin only)</summary>
    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<SystemAccountDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IQueryable>> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "Keyword must not be empty." });

        var accounts = await _service.SearchAsync(keyword);
        return Ok(accounts.AsQueryable());
    }

    /// <summary>GET a system account by ID (Admin or own profile)</summary>
    [HttpGet("({id:int})")]
    [Authorize(Roles = "Admin,Staff")]
    [EnableQuery]
    [ProducesResponseType(typeof(SystemAccountDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(typeof(SystemAccountDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post([FromBody] SystemAccountCreateDTO dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.AccountId }, created);
    }

    /// <summary>PUT update an existing system account (Admin or own profile)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(SystemAccountDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Put([FromRoute] short id, [FromBody] SystemAccountUpdateDTO dto)
    {
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

        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null)
            return NotFound(new { message = $"Account with ID {id} not found." });

        return Ok(updated);
    }

    /// <summary>DELETE a system account (Admin only) - fails if account has news articles</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] short id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = $"Account with ID {id} not found." });

        return NoContent();
    }

    /// <summary>POST login with email and password</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var result = await _service.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { success = false, message = "Invalid email or password." });

        return Ok(result);
    }
}
