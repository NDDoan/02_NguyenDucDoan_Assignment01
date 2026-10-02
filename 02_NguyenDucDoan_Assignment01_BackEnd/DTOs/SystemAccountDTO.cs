using System.ComponentModel.DataAnnotations;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;

public class SystemAccountDTO
{
    public short AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountEmail { get; set; }
    public int? AccountRole { get; set; }
}

public class SystemAccountCreateDTO
{
    [Required(ErrorMessage = "Account name is required.")]
    [MaxLength(100, ErrorMessage = "Account name must not exceed 100 characters.")]
    public string AccountName { get; set; } = null!;

    [Required(ErrorMessage = "Account email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(70, ErrorMessage = "Account email must not exceed 70 characters.")]
    public string AccountEmail { get; set; } = null!;

    [Required(ErrorMessage = "Account role is required.")]
    public int AccountRole { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(70, ErrorMessage = "Password must not exceed 70 characters.")]
    public string AccountPassword { get; set; } = null!;
}

public class SystemAccountUpdateDTO
{
    [Required(ErrorMessage = "Account name is required.")]
    [MaxLength(100, ErrorMessage = "Account name must not exceed 100 characters.")]
    public string AccountName { get; set; } = null!;

    [Required(ErrorMessage = "Account email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(70, ErrorMessage = "Account email must not exceed 70 characters.")]
    public string AccountEmail { get; set; } = null!;

    [Required(ErrorMessage = "Account role is required.")]
    public int AccountRole { get; set; }

    [MaxLength(70, ErrorMessage = "Password must not exceed 70 characters.")]
    public string? AccountPassword { get; set; }
}

public class LoginDTO
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = null!;
}

public class LoginResponseDTO
{
    public short AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountEmail { get; set; }
    public int? AccountRole { get; set; }
    public bool IsAdmin { get; set; }
}
