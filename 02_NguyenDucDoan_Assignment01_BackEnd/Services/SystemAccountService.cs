using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Services;

public class SystemAccountService : ISystemAccountService
{
    private readonly FunewsManagementContext _context;
    private readonly IConfiguration _configuration;

    public SystemAccountService(FunewsManagementContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private static SystemAccountDTO MapToDTO(SystemAccount account) => new()
    {
        AccountId = account.AccountId,
        AccountName = account.AccountName,
        AccountEmail = account.AccountEmail,
        AccountRole = account.AccountRole
    };

    public async Task<IEnumerable<SystemAccountDTO>> GetAllAsync()
    {
        var accounts = await _context.SystemAccounts.ToListAsync();
        return accounts.Select(MapToDTO);
    }

    public async Task<SystemAccountDTO?> GetByIdAsync(short id)
    {
        var account = await _context.SystemAccounts.FindAsync(id);
        return account == null ? null : MapToDTO(account);
    }

    public async Task<SystemAccountDTO> CreateAsync(SystemAccountCreateDTO dto)
    {
        if (await _context.SystemAccounts.AnyAsync(a => a.AccountEmail == dto.AccountEmail))
            throw new InvalidOperationException("Email is already in use.");

        // Generate next AccountId
        short nextId = 1;
        if (await _context.SystemAccounts.AnyAsync())
            nextId = (short)(await _context.SystemAccounts.MaxAsync(a => a.AccountId) + 1);

        var account = new SystemAccount
        {
            AccountId = nextId,
            AccountName = dto.AccountName,
            AccountEmail = dto.AccountEmail,
            AccountRole = dto.AccountRole,
            AccountPassword = dto.AccountPassword
        };

        _context.SystemAccounts.Add(account);
        await _context.SaveChangesAsync();
        return MapToDTO(account);
    }

    public async Task<SystemAccountDTO?> UpdateAsync(short id, SystemAccountUpdateDTO dto)
    {
        var account = await _context.SystemAccounts.FindAsync(id);
        if (account == null) return null;

        if (!string.Equals(account.AccountEmail, dto.AccountEmail, StringComparison.OrdinalIgnoreCase) &&
            await _context.SystemAccounts.AnyAsync(a => a.AccountEmail == dto.AccountEmail && a.AccountId != id))
        {
            throw new InvalidOperationException("Email is already in use by another account.");
        }

        account.AccountName = dto.AccountName;
        account.AccountEmail = dto.AccountEmail;
        account.AccountRole = dto.AccountRole;
        if (!string.IsNullOrWhiteSpace(dto.AccountPassword))
            account.AccountPassword = dto.AccountPassword;

        await _context.SaveChangesAsync();
        return MapToDTO(account);
    }

    public async Task<bool> DeleteAsync(short id)
    {
        var account = await _context.SystemAccounts
            .Include(a => a.NewsArticles)
            .FirstOrDefaultAsync(a => a.AccountId == id);

        if (account == null) return false;

        // Cannot delete if account has created any news articles
        if (account.NewsArticles.Any())
            throw new InvalidOperationException("Cannot delete account that has created news articles.");

        _context.SystemAccounts.Remove(account);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<LoginResponseDTO?> LoginAsync(LoginDTO dto)
    {
        // Check admin account from appsettings
        var adminEmail = _configuration["AdminAccount:Email"];
        var adminPassword = _configuration["AdminAccount:Password"];

        if (!string.IsNullOrEmpty(adminEmail) &&
            dto.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase) &&
            dto.Password == adminPassword)
        {
            return new LoginResponseDTO
            {
                AccountId = 0,
                AccountName = "Administrator",
                AccountEmail = adminEmail,
                AccountRole = 0,
                IsAdmin = true
            };
        }

        // Check database accounts
        var account = await _context.SystemAccounts
            .FirstOrDefaultAsync(a => a.AccountEmail == dto.Email && a.AccountPassword == dto.Password);

        if (account == null) return null;

        return new LoginResponseDTO
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            AccountRole = account.AccountRole,
            IsAdmin = false
        };
    }
}
