using SharedLife.Models.DTOs.Auth;
using SharedLife.Models.Entities;

namespace SharedLife.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message, LoginResponseDto? Data)> RegisterAsync(RegisterRequestDto request);
    Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginRequestDto request);
    Task<(bool Success, string Message, UserResponseDto? Data)> GetCurrentUserAsync(int userId);
    Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<bool> EmailExistsAsync(string email);
}
