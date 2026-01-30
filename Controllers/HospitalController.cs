using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLife.Models.DTOs.Hospital;
using SharedLife.Models.DTOs.Recipient;
using SharedLife.Models.Responses;
using SharedLife.Services.Interfaces;

namespace SharedLife.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HospitalController : ControllerBase
{
    private readonly IHospitalService _hospitalService;
    private readonly ILogger<HospitalController> _logger;

    public HospitalController(IHospitalService hospitalService, ILogger<HospitalController> logger)
    {
        _hospitalService = hospitalService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    #region Hospital Profile

    /// <summary>
    /// Register as a hospital (complete hospital profile)
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<ApiResponse<HospitalProfileDto>>> Register([FromBody] HospitalRegistrationDto request)
    {
        var userId = GetUserId();
        var (success, message, data) = await _hospitalService.RegisterHospitalAsync(userId, request);

        if (!success)
        {
            return BadRequest(ApiResponse<HospitalProfileDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<HospitalProfileDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Get current user's hospital profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<ApiResponse<HospitalProfileDto>>> GetProfile()
    {
        var userId = GetUserId();
        var (success, message, data) = await _hospitalService.GetHospitalProfileAsync(userId);

        if (!success)
        {
            return NotFound(ApiResponse<HospitalProfileDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<HospitalProfileDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Get hospital by ID (for admin viewing)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<HospitalProfileDto>>> GetById(int id)
    {
        var (success, message, data) = await _hospitalService.GetHospitalByIdAsync(id);

        if (!success)
        {
            return NotFound(ApiResponse<HospitalProfileDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<HospitalProfileDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Update hospital profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<ApiResponse<HospitalProfileDto>>> UpdateProfile([FromBody] HospitalUpdateDto request)
    {
        var userId = GetUserId();
        var (success, message, data) = await _hospitalService.UpdateHospitalAsync(userId, request);

        if (!success)
        {
            return BadRequest(ApiResponse<HospitalProfileDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<HospitalProfileDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Check if user has a hospital profile
    /// </summary>
    [HttpGet("status")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<ApiResponse<object>>> CheckStatus()
    {
        var userId = GetUserId();
        var hasProfile = await _hospitalService.IsHospitalAsync(userId);

        return Ok(ApiResponse<object>.SuccessResponse(
            new { hasProfile },
            hasProfile ? "Hospital profile exists" : "Hospital profile not found"
        ));
    }

    #endregion

    #region Dashboard

    /// <summary>
    /// Get hospital dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<ApiResponse<HospitalDashboardDto>>> GetDashboard()
    {
        var userId = GetUserId();
        var (success, message, data) = await _hospitalService.GetDashboardAsync(userId);

        if (!success)
        {
            return BadRequest(ApiResponse<HospitalDashboardDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<HospitalDashboardDto>.SuccessResponse(data!, message));
    }

    #endregion

    #region Donor Verification

    /// <summary>
    /// Get list of pending donors for verification
    /// </summary>
    [HttpGet("donors/pending")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<ApiResponse<List<PendingDonorDto>>>> GetPendingDonors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var (success, message, data) = await _hospitalService.GetPendingDonorsAsync(userId, page, pageSize);

        if (!success)
        {
            return BadRequest(ApiResponse<List<PendingDonorDto>>.ErrorResponse(message));
        }

        return Ok(ApiResponse<List<PendingDonorDto>>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Get list of donors verified by this hospital
    /// </summary>
    [HttpGet("donors/verified")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<ApiResponse<List<PendingDonorDto>>>> GetVerifiedDonors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var (success, message, data) = await _hospitalService.GetVerifiedDonorsAsync(userId, page, pageSize);

        if (!success)
        {
            return BadRequest(ApiResponse<List<PendingDonorDto>>.ErrorResponse(message));
        }

        return Ok(ApiResponse<List<PendingDonorDto>>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Verify a donor (approve, reject, etc.)
    /// </summary>
    [HttpPost("donors/verify")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyDonor([FromBody] DonorVerificationDto request)
    {
        var userId = GetUserId();
        var (success, message) = await _hospitalService.VerifyDonorAsync(userId, request);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    #endregion

    #region Donation Requests

    /// <summary>
    /// Get donation requests in hospital's area
    /// </summary>
    [HttpGet("requests")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<ApiResponse<List<DonationRequestDto>>>> GetAreaRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var userId = GetUserId();
        var (success, message, data) = await _hospitalService.GetAreaRequestsAsync(userId, page, pageSize, status);

        if (!success)
        {
            return BadRequest(ApiResponse<List<DonationRequestDto>>.ErrorResponse(message));
        }

        return Ok(ApiResponse<List<DonationRequestDto>>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Process a donation request (approve, complete, cancel)
    /// </summary>
    [HttpPost("requests/{requestId}/process")]
    [Authorize(Roles = "Hospital")]
    public async Task<ActionResult<ApiResponse<object>>> ProcessRequest(
        int requestId,
        [FromBody] ProcessRequestDto request)
    {
        var userId = GetUserId();
        var (success, message) = await _hospitalService.ProcessRequestAsync(userId, requestId, request.Action, request.Notes);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, message));
    }

    #endregion
}

public class ProcessRequestDto
{
    public string Action { get; set; } = string.Empty; // approve, complete, cancel
    public string? Notes { get; set; }
}
