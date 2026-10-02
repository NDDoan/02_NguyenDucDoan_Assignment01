using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDTO>> GetAllAsync();
    Task<IEnumerable<CategoryDTO>> GetActiveAsync();
    Task<CategoryDTO?> GetByIdAsync(short id);
    Task<CategoryDTO> CreateAsync(CategoryCreateDTO dto);
    Task<CategoryDTO?> UpdateAsync(short id, CategoryUpdateDTO dto);
    Task<bool> DeleteAsync(short id);
    Task<IEnumerable<CategoryDTO>> SearchAsync(string keyword);
}
