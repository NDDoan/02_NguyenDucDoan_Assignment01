using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using _02_NguyenDucDoan_Assignment01_BackEnd.Repositories;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
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
        var categories = await _repository.GetAllAsync();
        return categories.Select(MapToDTO);
    }

    public async Task<IEnumerable<CategoryDTO>> GetActiveAsync()
    {
        var categories = await _repository.GetActiveAsync();
        return categories.Select(MapToDTO);
    }

    public async Task<CategoryDTO?> GetByIdAsync(short id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category == null ? null : MapToDTO(category);
    }

    public async Task<CategoryDTO> CreateAsync(CategoryCreateDTO dto)
    {
        if (dto.ParentCategoryId.HasValue)
        {
            var parentExists = await _repository.ExistsAsync(dto.ParentCategoryId.Value);
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

        var created = await _repository.CreateAsync(category);
        return MapToDTO(created);
    }

    public async Task<CategoryDTO?> UpdateAsync(short id, CategoryUpdateDTO dto)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null) return null;

        if (dto.ParentCategoryId.HasValue)
        {
            if (dto.ParentCategoryId.Value == id)
                throw new InvalidOperationException("A category cannot be its own parent.");

            var parentExists = await _repository.ExistsAsync(dto.ParentCategoryId.Value);
            if (!parentExists)
                throw new InvalidOperationException($"Parent category with ID {dto.ParentCategoryId.Value} does not exist.");
        }

        category.CategoryName = dto.CategoryName;
        category.CategoryDesciption = dto.CategoryDesciption;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.IsActive = dto.IsActive;

        var updated = await _repository.UpdateAsync(category);
        return updated == null ? null : MapToDTO(updated);
    }

    public async Task<bool> DeleteAsync(short id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CategoryDTO>> SearchAsync(string keyword)
    {
        var categories = await _repository.SearchAsync(keyword);
        return categories.Select(MapToDTO);
    }
}
