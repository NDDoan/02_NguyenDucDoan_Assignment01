using _02_NguyenDucDoan_Assignment01_BackEnd.DAOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Models;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Repositories;

public interface ISystemAccountRepository
{
    Task<List<SystemAccount>> GetAllAsync();
    Task<SystemAccount?> GetByIdAsync(short id);
    Task<SystemAccount?> GetByEmailAsync(string email);
    Task<bool> ExistsEmailAsync(string email);
    Task<bool> ExistsEmailOtherIdAsync(string email, short accountId);
    Task<short> GetNextIdAsync();
    Task<SystemAccount> CreateAsync(SystemAccount account);
    Task<SystemAccount?> UpdateAsync(SystemAccount account);
    Task<bool> DeleteAsync(short id);
    Task<List<SystemAccount>> SearchAsync(string keyword);
}

public class SystemAccountRepository : ISystemAccountRepository
{
    public Task<List<SystemAccount>> GetAllAsync() => SystemAccountDAO.Instance.GetAllAsync();
    public Task<SystemAccount?> GetByIdAsync(short id) => SystemAccountDAO.Instance.GetByIdAsync(id);
    public Task<SystemAccount?> GetByEmailAsync(string email) => SystemAccountDAO.Instance.GetByEmailAsync(email);
    public Task<bool> ExistsEmailAsync(string email) => SystemAccountDAO.Instance.ExistsEmailAsync(email);
    public Task<bool> ExistsEmailOtherIdAsync(string email, short accountId) => SystemAccountDAO.Instance.ExistsEmailOtherIdAsync(email, accountId);
    public Task<short> GetNextIdAsync() => SystemAccountDAO.Instance.GetNextIdAsync();
    public Task<SystemAccount> CreateAsync(SystemAccount account) => SystemAccountDAO.Instance.CreateAsync(account);
    public Task<SystemAccount?> UpdateAsync(SystemAccount account) => SystemAccountDAO.Instance.UpdateAsync(account);
    public Task<bool> DeleteAsync(short id) => SystemAccountDAO.Instance.DeleteAsync(id);
    public Task<List<SystemAccount>> SearchAsync(string keyword) => SystemAccountDAO.Instance.SearchAsync(keyword);
}
