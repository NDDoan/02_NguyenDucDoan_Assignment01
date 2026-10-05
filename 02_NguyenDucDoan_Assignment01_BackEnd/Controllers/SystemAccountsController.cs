using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Controllers;

[Route("api/[controller]")]
public class SystemAccountsController : ODataController
{
    private readonly ISystemAccountService _service;

    public SystemAccountsController(ISystemAccountService service)
    {
        _service = service;
    }

    /// <summary>GET all system accounts (Admin only - Hỗ trợ OData Query: $filter, $select, $orderby, $top, $skip...)</summary>
    [HttpGet]
    [EnableQuery]
    public async Task<ActionResult<IQueryable>> Get()
    {
        var accounts = await _service.GetAllAsync();
        return Ok(accounts.AsQueryable());
    }

    /// <summary>GET a system account by ID using OData convention</summary>
    [HttpGet("({id:int})")]
    [EnableQuery]
    public async Task<IActionResult> GetById([FromRoute] short id)
    {
        var account = await _service.GetByIdAsync(id);
        if (account == null)
            return NotFound(new { message = $"Account with ID {id} not found." });

        return Ok(account);
    }

    /// <summary>POST create a new system account (Admin only)</summary>
    [HttpPost]
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
            return Conflict(new { message = ex.Message }); // Bắt lỗi trùng Email
        }
    }

    /// <summary>PUT update an existing system account (Admin only)</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put([FromRoute] short id, [FromBody] SystemAccountUpdateDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = $"Account with ID {id} not found." });

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message }); // Bắt lỗi trùng Email khi update
        }
    }

    /// <summary>DELETE a system account (Admin only) - fails if account has news articles</summary>
    [HttpDelete("{id:int}")]
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

    /// <summary>POST login with email and password (Custom action, giữ nguyên logic chuẩn)</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _service.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(result);
    }
}