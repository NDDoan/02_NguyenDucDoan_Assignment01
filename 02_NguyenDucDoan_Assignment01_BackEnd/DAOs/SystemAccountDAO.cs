using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.DAOs;

public class SystemAccountDAO
{
    private static SystemAccountDAO? _instance;
    private static readonly object _lock = new();

    private SystemAccountDAO() { }

    public static SystemAccountDAO Instance
    {
        get
        {
            lock (_lock)
            {
                return _instance ??= new SystemAccountDAO();
            }
        }
    }

    public async Task<List<SystemAccount>> GetAllAsync()
    {
        using var context = new FunewsManagementContext();
        return await context.SystemAccounts.ToListAsync();
    }

    public async Task<SystemAccount?> GetByIdAsync(short id)
    {
        using var context = new FunewsManagementContext();
        return await context.SystemAccounts.FindAsync(id);
    }

    public async Task<SystemAccount?> GetByEmailAsync(string email)
    {
        using var context = new FunewsManagementContext();
        return await context.SystemAccounts.FirstOrDefaultAsync(a => a.AccountEmail == email);
    }

    public async Task<bool> ExistsEmailAsync(string email)
    {
        using var context = new FunewsManagementContext();
        return await context.SystemAccounts.AnyAsync(a => a.AccountEmail == email);
    }

    public async Task<bool> ExistsEmailOtherIdAsync(string email, short accountId)
    {
        using var context = new FunewsManagementContext();
        return await context.SystemAccounts.AnyAsync(a => a.AccountEmail == email && a.AccountId != accountId);
    }

    public async Task<short> GetNextIdAsync()
    {
        using var context = new FunewsManagementContext();
        if (!await context.SystemAccounts.AnyAsync()) return 1;
        return (short)(await context.SystemAccounts.MaxAsync(a => a.AccountId) + 1);
    }

    public async Task<SystemAccount> CreateAsync(SystemAccount account)
    {
        using var context = new FunewsManagementContext();
        context.SystemAccounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }

    public async Task<SystemAccount?> UpdateAsync(SystemAccount account)
    {
        using var context = new FunewsManagementContext();
        var existing = await context.SystemAccounts.FindAsync(account.AccountId);
        if (existing == null) return null;

        existing.AccountName = account.AccountName;
        existing.AccountEmail = account.AccountEmail;
        existing.AccountRole = account.AccountRole;
        if (!string.IsNullOrWhiteSpace(account.AccountPassword))
        {
            existing.AccountPassword = account.AccountPassword;
        }

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(short id)
    {
        using var context = new FunewsManagementContext();
        var account = await context.SystemAccounts
            .Include(a => a.NewsArticles)
            .FirstOrDefaultAsync(a => a.AccountId == id);

        if (account == null) return false;

        if (account.NewsArticles.Any())
            throw new InvalidOperationException("Cannot delete account that has created news articles.");

        context.SystemAccounts.Remove(account);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SystemAccount>> SearchAsync(string keyword)
    {
        using var context = new FunewsManagementContext();
        return await context.SystemAccounts
            .Where(a => (a.AccountName != null && a.AccountName.Contains(keyword)) ||
                        (a.AccountEmail != null && a.AccountEmail.Contains(keyword)))
            .ToListAsync();
    }
}
