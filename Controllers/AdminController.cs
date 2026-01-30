using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLife.Models.DTOs.Admin;
using SharedLife.Models.Responses;
using SharedLife.Services.Interfaces;

namespace SharedLife.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    #region Dashboard

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<DashboardStatsDto>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(result.Data!, result.Message));
    }

    #endregion

    #region User Management

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<UserListResponseDto>>> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await _adminService.GetAllUsersAsync(page, pageSize, search, role);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<UserListResponseDto>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<UserListResponseDto>.SuccessResponse(result.Data!, result.Message));
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("users/{id}")]
    public async Task<ActionResult<ApiResponse<UserDetailsDto>>> GetUserById(int id)
    {
        var result = await _adminService.GetUserByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(ApiResponse<UserDetailsDto>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<UserDetailsDto>.SuccessResponse(result.Data!, result.Message));
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("users/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var result = await _adminService.UpdateUserAsync(id, dto);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, result.Message));
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    [HttpDelete("users/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
    {
        var result = await _adminService.DeleteUserAsync(id);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, result.Message));
    }

    /// <summary>
    /// Verify user
    /// </summary>
    [HttpPost("users/{id}/verify")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyUser(int id)
    {
        var result = await _adminService.VerifyUserAsync(id);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, result.Message));
    }

    #endregion

    #region Donor Management

    /// <summary>
    /// Get all donors with pagination and filters
    /// </summary>
    [HttpGet("donors")]
    public async Task<ActionResult<ApiResponse<DonorListResponseDto>>> GetAllDonors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? bloodGroup = null,
        [FromQuery] bool? isAvailable = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await _adminService.GetAllDonorsAsync(page, pageSize, search, bloodGroup, isAvailable);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<DonorListResponseDto>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<DonorListResponseDto>.SuccessResponse(result.Data!, result.Message));
    }

    /// <summary>
    /// Verify donor
    /// </summary>
    [HttpPost("donors/{id}/verify")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyDonor(int id)
    {
        var result = await _adminService.VerifyDonorAsync(id);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, result.Message));
    }

    #endregion

    #region Recipient Management

    /// <summary>
    /// Get all recipients with pagination and filters
    /// </summary>
    [HttpGet("recipients")]
    public async Task<ActionResult<ApiResponse<RecipientListResponseDto>>> GetAllRecipients(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? bloodGroup = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await _adminService.GetAllRecipientsAsync(page, pageSize, search, bloodGroup);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<RecipientListResponseDto>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<RecipientListResponseDto>.SuccessResponse(result.Data!, result.Message));
    }

    /// <summary>
    /// Verify recipient
    /// </summary>
    [HttpPost("recipients/{id}/verify")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyRecipient(int id)
    {
        var result = await _adminService.VerifyRecipientAsync(id);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, result.Message));
    }

    #endregion

    #region Request Management

    /// <summary>
    /// Get all donation requests with pagination and filters
    /// </summary>
    [HttpGet("requests")]
    public async Task<ActionResult<ApiResponse<RequestListResponseDto>>> GetAllRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? urgencyLevel = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await _adminService.GetAllRequestsAsync(page, pageSize, status, urgencyLevel);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<RequestListResponseDto>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<RequestListResponseDto>.SuccessResponse(result.Data!, result.Message));
    }

    /// <summary>
    /// Update request status
    /// </summary>
    [HttpPut("requests/{id}/status")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateRequestStatus(int id, [FromBody] UpdateRequestStatusDto dto)
    {
        var result = await _adminService.UpdateRequestStatusAsync(id, dto);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, result.Message));
    }

    #endregion

    #region Hospital Management

    /// <summary>
    /// Get all hospitals with pagination and filters
    /// </summary>
    [HttpGet("hospitals")]
    public async Task<ActionResult<ApiResponse<HospitalListResponseDto>>> GetAllHospitals(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isVerified = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await _adminService.GetAllHospitalsAsync(page, pageSize, search, isVerified);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<HospitalListResponseDto>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<HospitalListResponseDto>.SuccessResponse(result.Data!, result.Message));
    }

    /// <summary>
    /// Verify hospital
    /// </summary>
    [HttpPost("hospitals/{id}/verify")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyHospital(int id, [FromBody] VerifyHospitalDto? dto = null)
    {
        var result = await _adminService.VerifyHospitalAsync(id, dto?.Notes);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, result.Message));
    }

    #endregion
}
