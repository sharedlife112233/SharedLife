using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLife.Models.DTOs.Auth;
using SharedLife.Models.Responses;
using SharedLife.Services.Interfaces;

namespace SharedLife.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return Ok(ApiResponse<LoginResponseDto>.ErrorResponse("Validation failed", errors));
        }

        var (success, message, data) = await _authService.RegisterAsync(request);

        if (!success)
        {
            return Ok(ApiResponse<LoginResponseDto>.ErrorResponse(message));
        }

        // Hospital registration returns success but no login data (pending approval)
        if (data == null)
        {
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(null!, message));
        }

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(data, message));
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse("Validation failed", errors));
        }

        var (success, message, data) = await _authService.LoginAsync(request);

        if (!success)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Get current authenticated user
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                          ?? User.FindFirst("sub");
        
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResponse<UserResponseDto>.ErrorResponse("Invalid token"));
        }

        var (success, message, data) = await _authService.GetCurrentUserAsync(userId);

        if (!success)
        {
            return NotFound(ApiResponse<UserResponseDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var (success, message, data) = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!success)
        {
            return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.RevokeTokenAsync(request.RefreshToken);
        return Ok(new { Success = true, Message = "Logged out successfully" });
    }

    /// <summary>
    /// Check if email is already registered
    /// </summary>
    [HttpGet("check-email")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
    {
        try
        {
            var exists = await _authService.EmailExistsAsync(email);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability for {Email}", email);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse("Unable to check email availability right now"));
        }
    }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
