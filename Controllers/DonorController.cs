using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLife.Models.DTOs.Donor;
using SharedLife.Models.Responses;
using SharedLife.Services.Interfaces;

namespace SharedLife.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonorController : ControllerBase
{
    private readonly IDonorService _donorService;
    private readonly ILogger<DonorController> _logger;

    public DonorController(IDonorService donorService, ILogger<DonorController> logger)
    {
        _donorService = donorService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    /// <summary>
    /// Register as a donor (complete donor profile)
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<ApiResponse<DonorProfileDto>>> Register([FromBody] DonorRegistrationDto request)
    {
        var userId = GetUserId();
        var (success, message, data) = await _donorService.RegisterDonorAsync(userId, request);

        if (!success)
        {
            return BadRequest(ApiResponse<DonorProfileDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<DonorProfileDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Get current user's donor profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<ApiResponse<DonorProfileDto>>> GetProfile()
    {
        var userId = GetUserId();
        var (success, message, data) = await _donorService.GetDonorProfileAsync(userId);

        if (!success)
        {
            return NotFound(ApiResponse<DonorProfileDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<DonorProfileDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Get donor by ID (for staff/admin viewing)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ApiResponse<DonorProfileDto>>> GetById(int id)
    {
        var (success, message, data) = await _donorService.GetDonorByIdAsync(id);

        if (!success)
        {
            return NotFound(ApiResponse<DonorProfileDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<DonorProfileDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Update donor profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<ApiResponse<DonorProfileDto>>> UpdateProfile([FromBody] DonorUpdateDto request)
    {
        var userId = GetUserId();
        var (success, message, data) = await _donorService.UpdateDonorAsync(userId, request);

        if (!success)
        {
            return BadRequest(ApiResponse<DonorProfileDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<DonorProfileDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Update availability status
    /// </summary>
    [HttpPatch("availability")]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateAvailability([FromBody] DonorAvailabilityDto request)
    {
        var userId = GetUserId();
        var (success, message) = await _donorService.UpdateAvailabilityAsync(userId, request);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, message));
    }

    /// <summary>
    /// Get all donors (with optional filters)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ApiResponse<List<DonorListItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? bloodGroup = null,
        [FromQuery] bool? isAvailable = null)
    {
        var (success, message, data) = await _donorService.GetAllDonorsAsync(page, pageSize, bloodGroup, isAvailable);

        if (!success)
        {
            return BadRequest(ApiResponse<List<DonorListItemDto>>.ErrorResponse(message));
        }

        return Ok(ApiResponse<List<DonorListItemDto>>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Check if current user has completed donor registration
    /// </summary>
    [HttpGet("status")]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<ApiResponse<object>>> CheckDonorStatus()
    {
        var userId = GetUserId();
        var isDonor = await _donorService.IsDonorAsync(userId);

        var data = new { HasDonorProfile = isDonor };
        return Ok(ApiResponse<object>.SuccessResponse(data, 
            isDonor ? "Donor profile exists" : "Donor profile not found"));
    }

    /// <summary>
    /// Record a blood donation (staff only)
    /// </summary>
    [HttpPost("{userId}/record-donation")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ApiResponse<object>>> RecordDonation(int userId, [FromBody] DateTime donationDate)
    {
        var (success, message) = await _donorService.RecordBloodDonationAsync(userId, donationDate);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, message));
    }

    /// <summary>
    /// Get incoming donation requests that match the donor's blood type
    /// </summary>
    [HttpGet("requests/incoming")]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<ApiResponse<List<Models.DTOs.Recipient.IncomingDonationRequestDto>>>> GetIncomingRequests()
    {
        var userId = GetUserId();
        var (success, message, data) = await _donorService.GetIncomingRequestsAsync(userId);

        if (!success)
        {
            return BadRequest(ApiResponse<List<Models.DTOs.Recipient.IncomingDonationRequestDto>>.ErrorResponse(message));
        }

        return Ok(ApiResponse<List<Models.DTOs.Recipient.IncomingDonationRequestDto>>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Get the donor's own donation history
    /// </summary>
    [HttpGet("history")]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<ApiResponse<List<DonorDonationHistoryDto>>>> GetDonationHistory()
    {
        var userId = GetUserId();
        var (success, message, data) = await _donorService.GetDonationHistoryAsync(userId);

        if (!success)
        {
            return BadRequest(ApiResponse<List<DonorDonationHistoryDto>>.ErrorResponse(message));
        }

        return Ok(ApiResponse<List<DonorDonationHistoryDto>>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Respond to a donation request (accept or decline)
    /// </summary>
    [HttpPost("requests/{requestId}/respond")]
    [Authorize(Roles = "Donor")]
    public async Task<ActionResult<ApiResponse<object>>> RespondToRequest(int requestId, [FromBody] DonorResponseDto response)
    {
        var userId = GetUserId();
        var (success, message) = await _donorService.RespondToRequestAsync(userId, requestId, response.Accept, response.Notes);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, message));
    }
}

public class DonorResponseDto
{
    public bool Accept { get; set; }
    public string? Notes { get; set; }
}
