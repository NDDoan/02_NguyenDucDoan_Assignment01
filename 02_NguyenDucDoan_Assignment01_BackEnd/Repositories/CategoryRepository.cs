using _02_NguyenDucDoan_Assignment01_BackEnd.DAOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Models;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<List<Category>> GetActiveAsync();
    Task<Category?> GetByIdAsync(short id);
    Task<bool> ExistsAsync(short id);
    Task<Category> CreateAsync(Category category);
    Task<Category?> UpdateAsync(Category category);
    Task<bool> DeleteAsync(short id);
    Task<List<Category>> SearchAsync(string keyword);
}

public class CategoryRepository : ICategoryRepository
{
    public Task<List<Category>> GetAllAsync() => CategoryDAO.Instance.GetAllAsync();
    public Task<List<Category>> GetActiveAsync() => CategoryDAO.Instance.GetActiveAsync();
    public Task<Category?> GetByIdAsync(short id) => CategoryDAO.Instance.GetByIdAsync(id);
    public Task<bool> ExistsAsync(short id) => CategoryDAO.Instance.ExistsAsync(id);
    public Task<Category> CreateAsync(Category category) => CategoryDAO.Instance.CreateAsync(category);
    public Task<Category?> UpdateAsync(Category category) => CategoryDAO.Instance.UpdateAsync(category);
    public Task<bool> DeleteAsync(short id) => CategoryDAO.Instance.DeleteAsync(id);
    public Task<List<Category>> SearchAsync(string keyword) => CategoryDAO.Instance.SearchAsync(keyword);
}
