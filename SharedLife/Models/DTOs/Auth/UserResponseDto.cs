using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Auth;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public string? Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
