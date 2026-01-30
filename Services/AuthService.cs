using Microsoft.EntityFrameworkCore;
using SharedLife.Data;
using SharedLife.Models.DTOs.Auth;
using SharedLife.Models.Entities;
using SharedLife.Services.Interfaces;
using SharedLife.Utilities;

namespace SharedLife.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context, 
        JwtHelper jwtHelper,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            // Check if email already exists
            if (await EmailExistsAsync(request.Email))
            {
                return (false, "Email is already registered", null);
            }

            // Create new user
            var user = new User
            {
                Email = request.Email.ToLower().Trim(),
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Role = request.Role,
                BloodGroup = request.BloodGroup,
                Address = request.Address?.Trim(),
                DateOfBirth = request.DateOfBirth,
                IsActive = true,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate tokens
            var token = _jwtHelper.GenerateJwtToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            var response = new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                RefreshToken = refreshToken.Token,
                TokenExpiration = _jwtHelper.GetTokenExpiration()
            };

            _logger.LogInformation("User registered successfully: {Email}", user.Email);
            return (true, "Registration successful", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return (false, "An error occurred during registration", null);
        }
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());

            if (user == null)
            {
                return (false, "Invalid email or password", null);
            }

            if (!user.IsActive)
            {
                return (false, "Account is deactivated. Please contact support.", null);
            }

            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return (false, "Invalid email or password", null);
            }

            // Generate tokens
            var token = _jwtHelper.GenerateJwtToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            var response = new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                RefreshToken = refreshToken.Token,
                TokenExpiration = _jwtHelper.GetTokenExpiration()
            };

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);
            return (true, "Login successful", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return (false, "An error occurred during login", null);
        }
    }

    public async Task<(bool Success, string Message, UserResponseDto? Data)> GetCurrentUserAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return (false, "User not found", null);
            }

            var response = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                BloodGroup = user.BloodGroup,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt
            };

            return (true, "User retrieved successfully", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", userId);
            return (false, "An error occurred while retrieving user data", null);
        }
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var token = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (token == null || !token.IsActive)
            {
                return (false, "Invalid or expired refresh token", null);
            }

            // Revoke old token
            token.RevokedAt = DateTime.UtcNow;

            // Generate new tokens
            var newJwtToken = _jwtHelper.GenerateJwtToken(token.User);
            var newRefreshToken = await CreateRefreshTokenAsync(token.UserId);

            await _context.SaveChangesAsync();

            var response = new LoginResponseDto
            {
                UserId = token.User.Id,
                Email = token.User.Email,
                FullName = token.User.FullName,
                Role = token.User.Role,
                Token = newJwtToken,
                RefreshToken = newRefreshToken.Token,
                TokenExpiration = _jwtHelper.GetTokenExpiration()
            };

            return (true, "Token refreshed successfully", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return (false, "An error occurred while refreshing token", null);
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        try
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (token == null || !token.IsActive)
            {
                return false;
            }

            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return false;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email.ToLower().Trim());
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(int userId)
    {
        var refreshToken = new RefreshToken
        {
            Token = _jwtHelper.GenerateRefreshToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}
