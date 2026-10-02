using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Services;

public interface ISystemAccountService
{
    Task<IEnumerable<SystemAccountDTO>> GetAllAsync();
    Task<SystemAccountDTO?> GetByIdAsync(short id);
    Task<SystemAccountDTO> CreateAsync(SystemAccountCreateDTO dto);
    Task<SystemAccountDTO?> UpdateAsync(short id, SystemAccountUpdateDTO dto);
    Task<bool> DeleteAsync(short id);
    Task<LoginResponseDTO?> LoginAsync(LoginDTO dto);
}
