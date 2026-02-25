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

            // Validate role-specific fields
            if (request.Role == Models.Enums.UserRole.Hospital)
            {
                if (string.IsNullOrWhiteSpace(request.HospitalName))
                    return (false, "Hospital name is required", null);
                if (string.IsNullOrWhiteSpace(request.RegistrationNumber))
                    return (false, "Hospital registration number is required", null);
                if (string.IsNullOrWhiteSpace(request.HospitalType))
                    return (false, "Hospital type is required", null);
            }
            else
            {
                // For Donor/Recipient, date of birth is required
                if (!request.DateOfBirth.HasValue)
                    return (false, "Date of birth is required", null);
            }

            // Create new user
            var user = new User
            {
                Email = request.Email.ToLower().Trim(),
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                FullName = request.Role == Models.Enums.UserRole.Hospital 
                    ? request.HospitalName!.Trim() 
                    : request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Role = request.Role,
                BloodGroup = request.BloodGroup,
                Address = request.Address?.Trim(),
                DateOfBirth = request.DateOfBirth ?? TimeHelper.Now, // Default for hospitals
                IsActive = true,
                IsVerified = false,
                CreatedAt = TimeHelper.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // If registering as Hospital, create the Hospital profile automatically
            if (request.Role == Models.Enums.UserRole.Hospital)
            {
                var hospital = new Hospital
                {
                    UserId = user.Id,
                    HospitalName = request.HospitalName!.Trim(),
                    RegistrationNumber = request.RegistrationNumber!.Trim(),
                    HospitalType = request.HospitalType!.Trim(),
                    Address = request.Address?.Trim() ?? string.Empty,
                    City = request.City?.Trim() ?? string.Empty,
                    State = request.State?.Trim() ?? string.Empty,
                    PinCode = string.Empty,
                    ContactEmail = request.Email.ToLower().Trim(),
                    ContactPhone = request.PhoneNumber.Trim(),
                    IsVerified = false,
                    IsActive = true,
                    CreatedAt = TimeHelper.Now
                };

                _context.Hospitals.Add(hospital);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Hospital profile created for user: {Email}", user.Email);

                // Hospitals must wait for admin approval before logging in
                return (true, "Registration successful! Your hospital account is pending admin approval. You will be able to log in once an admin verifies your hospital.", null);
            }

            // Generate tokens (for non-hospital users)
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

            // Check if hospital is verified before allowing login
            if (user.Role == Models.Enums.UserRole.Hospital)
            {
                var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.UserId == user.Id);
                if (hospital == null || !hospital.IsVerified)
                {
                    return (false, "Your hospital account is pending admin approval. Please wait for verification before logging in.", null);
                }
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
            token.RevokedAt = TimeHelper.Now;

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

            token.RevokedAt = TimeHelper.Now;
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
            ExpiresAt = TimeHelper.Now.AddDays(7),
            CreatedAt = TimeHelper.Now
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}
