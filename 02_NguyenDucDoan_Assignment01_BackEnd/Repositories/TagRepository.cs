using _02_NguyenDucDoan_Assignment01_BackEnd.DAOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Models;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Repositories;

public interface ITagRepository
{
    Task<List<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(int id);
    Task<List<Tag>> GetByIdsAsync(IEnumerable<int> ids);
}

public class TagRepository : ITagRepository
{
    public Task<List<Tag>> GetAllAsync() => TagDAO.Instance.GetAllAsync();
    public Task<Tag?> GetByIdAsync(int id) => TagDAO.Instance.GetByIdAsync(id);
    public Task<List<Tag>> GetByIdsAsync(IEnumerable<int> ids) => TagDAO.Instance.GetByIdsAsync(ids);
}
