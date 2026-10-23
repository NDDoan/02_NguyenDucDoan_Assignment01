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
        var jwtKey = _configuration["Jwt:Key"] ?? "this_is_a_fallback_secret_key_that_is_long_enough";

        if (!string.IsNullOrEmpty(adminEmail) &&
            dto.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase) &&
            dto.Password == adminPassword)
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(jwtKey);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "0"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, adminEmail),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Administrator"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new LoginResponseDTO
            {
                AccountId = 0,
                AccountName = "Administrator",
                AccountEmail = adminEmail,
                AccountRole = 0,
                IsAdmin = true,
                Token = tokenHandler.WriteToken(token)
            };
        }

        // Check database accounts
        var account = await _context.SystemAccounts
            .FirstOrDefaultAsync(a => a.AccountEmail == dto.Email && a.AccountPassword == dto.Password);

        if (account == null) return null;

        var userTokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var userKey = System.Text.Encoding.ASCII.GetBytes(jwtKey);
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, account.AccountId.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, account.AccountEmail),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, account.AccountName ?? "")
        };

        if (account.AccountRole == 1)
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Staff"));
        else if (account.AccountRole == 2)
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Lecturer"));

        var userTokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(userKey), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        var userToken = userTokenHandler.CreateToken(userTokenDescriptor);

        return new LoginResponseDTO
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            AccountRole = account.AccountRole,
            IsAdmin = false,
            Token = userTokenHandler.WriteToken(userToken)
        };
    }
}
