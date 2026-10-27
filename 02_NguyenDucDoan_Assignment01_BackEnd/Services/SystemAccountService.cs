using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using _02_NguyenDucDoan_Assignment01_BackEnd.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Services;

public class SystemAccountService : ISystemAccountService
{
    private readonly ISystemAccountRepository _repository;
    private readonly IConfiguration _configuration;

    public SystemAccountService(ISystemAccountRepository repository, IConfiguration configuration)
    {
        _repository = repository;
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
        var accounts = await _repository.GetAllAsync();
        return accounts.Select(MapToDTO);
    }

    public async Task<SystemAccountDTO?> GetByIdAsync(short id)
    {
        var account = await _repository.GetByIdAsync(id);
        return account == null ? null : MapToDTO(account);
    }

    public async Task<SystemAccountDTO> CreateAsync(SystemAccountCreateDTO dto)
    {
        if (await _repository.ExistsEmailAsync(dto.AccountEmail))
            throw new InvalidOperationException("Email is already in use.");

        short nextId = await _repository.GetNextIdAsync();

        var account = new SystemAccount
        {
            AccountId = nextId,
            AccountName = dto.AccountName,
            AccountEmail = dto.AccountEmail,
            AccountRole = dto.AccountRole,
            AccountPassword = dto.AccountPassword
        };

        var created = await _repository.CreateAsync(account);
        return MapToDTO(created);
    }

    public async Task<SystemAccountDTO?> UpdateAsync(short id, SystemAccountUpdateDTO dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        if (await _repository.ExistsEmailOtherIdAsync(dto.AccountEmail, id))
            throw new InvalidOperationException("Email is already in use by another account.");

        existing.AccountName = dto.AccountName;
        existing.AccountEmail = dto.AccountEmail;
        existing.AccountRole = dto.AccountRole;
        if (!string.IsNullOrWhiteSpace(dto.AccountPassword))
            existing.AccountPassword = dto.AccountPassword;

        var updated = await _repository.UpdateAsync(existing);
        return updated == null ? null : MapToDTO(updated);
    }

    public async Task<bool> DeleteAsync(short id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<SystemAccountDTO>> SearchAsync(string keyword)
    {
        var accounts = await _repository.SearchAsync(keyword);
        return accounts.Select(MapToDTO);
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
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "0"),
                    new Claim(ClaimTypes.Email, adminEmail),
                    new Claim(ClaimTypes.Name, "Administrator"),
                    new Claim(ClaimTypes.Role, "Admin")
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
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
        var account = await _repository.GetByEmailAsync(dto.Email);
        if (account == null || account.AccountPassword != dto.Password)
            return null;

        var userTokenHandler = new JwtSecurityTokenHandler();
        var userKey = Encoding.ASCII.GetBytes(jwtKey);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
            new Claim(ClaimTypes.Email, account.AccountEmail ?? ""),
            new Claim(ClaimTypes.Name, account.AccountName ?? "")
        };

        if (account.AccountRole == 1)
            claims.Add(new Claim(ClaimTypes.Role, "Staff"));
        else if (account.AccountRole == 2)
            claims.Add(new Claim(ClaimTypes.Role, "Lecturer"));

        var userTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(userKey), SecurityAlgorithms.HmacSha256Signature)
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
