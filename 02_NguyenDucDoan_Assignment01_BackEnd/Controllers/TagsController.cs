using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagsController : ControllerBase
{
    private readonly ITagRepository _tagRepository;

    public TagsController(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    /// <summary>GET all tags</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDTO>>> GetAll()
    {
        var tags = await _tagRepository.GetAllAsync();
        var dtos = tags.Select(t => new TagDTO
        {
            TagId = t.TagId,
            TagName = t.TagName,
            Note = t.Note
        });
        return Ok(dtos);
    }

    /// <summary>GET tag by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TagDTO>> GetById(int id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null) return NotFound(new { message = $"Tag with ID {id} not found." });

        return Ok(new TagDTO
        {
            TagId = tag.TagId,
            TagName = tag.TagName,
            Note = tag.Note
        });
    }
}
