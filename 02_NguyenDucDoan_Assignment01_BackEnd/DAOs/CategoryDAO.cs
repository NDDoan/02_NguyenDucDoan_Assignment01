using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.DAOs;

public class CategoryDAO
{
    private static CategoryDAO? _instance;
    private static readonly object _lock = new();

    private CategoryDAO() { }

    public static CategoryDAO Instance
    {
        get
        {
            lock (_lock)
            {
                return _instance ??= new CategoryDAO();
            }
        }
    }

    public async Task<List<Category>> GetAllAsync()
    {
        using var context = new FunewsManagementContext();
        return await context.Categories.ToListAsync();
    }

    public async Task<List<Category>> GetActiveAsync()
    {
        using var context = new FunewsManagementContext();
        return await context.Categories
            .Where(c => c.IsActive == true)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(short id)
    {
        using var context = new FunewsManagementContext();
        return await context.Categories.FindAsync(id);
    }

    public async Task<bool> ExistsAsync(short id)
    {
        using var context = new FunewsManagementContext();
        return await context.Categories.AnyAsync(c => c.CategoryId == id);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        using var context = new FunewsManagementContext();
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateAsync(Category category)
    {
        using var context = new FunewsManagementContext();
        var existing = await context.Categories.FindAsync(category.CategoryId);
        if (existing == null) return null;

        existing.CategoryName = category.CategoryName;
        existing.CategoryDesciption = category.CategoryDesciption;
        existing.ParentCategoryId = category.ParentCategoryId;
        existing.IsActive = category.IsActive;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(short id)
    {
        using var context = new FunewsManagementContext();
        var category = await context.Categories
            .Include(c => c.NewsArticles)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null) return false;

        if (category.NewsArticles.Any())
            throw new InvalidOperationException("Cannot delete category that is already used by news articles.");

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Category>> SearchAsync(string keyword)
    {
        using var context = new FunewsManagementContext();
        return await context.Categories
            .Where(c => c.CategoryName.Contains(keyword) || 
                       (c.CategoryDesciption != null && c.CategoryDesciption.Contains(keyword)))
            .ToListAsync();
    }
}
