using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Services;

public class CategoryService : ICategoryService
{
    private readonly FunewsManagementContext _context;

    public CategoryService(FunewsManagementContext context)
    {
        _context = context;
    }

    private static CategoryDTO MapToDTO(Category category) => new()
    {
        CategoryId = category.CategoryId,
        CategoryName = category.CategoryName,
        CategoryDesciption = category.CategoryDesciption,
        ParentCategoryId = category.ParentCategoryId,
        IsActive = category.IsActive
    };

    public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        return categories.Select(MapToDTO);
    }

    public async Task<IEnumerable<CategoryDTO>> GetActiveAsync()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive == true)
            .ToListAsync();
        return categories.Select(MapToDTO);
    }

    public async Task<CategoryDTO?> GetByIdAsync(short id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category == null ? null : MapToDTO(category);
    }

    public async Task<CategoryDTO> CreateAsync(CategoryCreateDTO dto)
    {
        if (dto.ParentCategoryId.HasValue)
        {
            var parentExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.ParentCategoryId.Value);
            if (!parentExists)
                throw new InvalidOperationException($"Parent category with ID {dto.ParentCategoryId.Value} does not exist.");
        }

        var category = new Category
        {
            CategoryName = dto.CategoryName,
            CategoryDesciption = dto.CategoryDesciption,
            ParentCategoryId = dto.ParentCategoryId,
            IsActive = dto.IsActive ?? true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return MapToDTO(category);
    }

    public async Task<CategoryDTO?> UpdateAsync(short id, CategoryUpdateDTO dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        if (dto.ParentCategoryId.HasValue)
        {
            if (dto.ParentCategoryId.Value == id)
                throw new InvalidOperationException("A category cannot be its own parent.");

            var parentExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.ParentCategoryId.Value);
            if (!parentExists)
                throw new InvalidOperationException($"Parent category with ID {dto.ParentCategoryId.Value} does not exist.");
        }



        category.CategoryName = dto.CategoryName;
        category.CategoryDesciption = dto.CategoryDesciption;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return MapToDTO(category);
    }

    public async Task<bool> DeleteAsync(short id)
    {
        var category = await _context.Categories
            .Include(c => c.NewsArticles)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null) return false;

        // Cannot delete if category has news articles
        if (category.NewsArticles.Any())
            throw new InvalidOperationException("Cannot delete category that is already used by news articles.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CategoryDTO>> SearchAsync(string keyword)
    {
        var categories = await _context.Categories
            .Where(c => c.CategoryName.Contains(keyword) || c.CategoryDesciption.Contains(keyword))
            .ToListAsync();
        return categories.Select(MapToDTO);
    }
}
