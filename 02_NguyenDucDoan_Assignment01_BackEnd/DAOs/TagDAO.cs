using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.DAOs;

public class TagDAO
{
    private static TagDAO? _instance;
    private static readonly object _lock = new();

    private TagDAO() { }

    public static TagDAO Instance
    {
        get
        {
            lock (_lock)
            {
                return _instance ??= new TagDAO();
            }
        }
    }

    public async Task<List<Tag>> GetAllAsync()
    {
        using var context = new FunewsManagementContext();
        return await context.Tags.ToListAsync();
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        using var context = new FunewsManagementContext();
        return await context.Tags.FindAsync(id);
    }

    public async Task<List<Tag>> GetByIdsAsync(IEnumerable<int> ids)
    {
        using var context = new FunewsManagementContext();
        return await context.Tags.Where(t => ids.Contains(t.TagId)).ToListAsync();
    }
}
