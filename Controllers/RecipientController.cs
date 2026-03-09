using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLife.Models.DTOs.Recipient;
using SharedLife.Models.Enums;
using SharedLife.Models.Responses;
using SharedLife.Services.Interfaces;

namespace SharedLife.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecipientController : ControllerBase
{
    private readonly IRecipientService _recipientService;
    private readonly ILogger<RecipientController> _logger;

    public RecipientController(IRecipientService recipientService, ILogger<RecipientController> logger)
    {
        _recipientService = recipientService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    #region Recipient Profile

    /// <summary>
    /// Check if current user has a recipient profile
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<RecipientStatusDto>>> CheckStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, message, data) = await _recipientService.CheckRecipientStatusAsync(userId);
            return Ok(new ApiResponse<RecipientStatusDto>(success, message, data));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<RecipientStatusDto>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking recipient status");
            return StatusCode(500, new ApiResponse<RecipientStatusDto>(false, "An error occurred"));
        }
    }

    /// <summary>
    /// Get current user's recipient profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<RecipientProfileDto>>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, message, data) = await _recipientService.GetRecipientProfileAsync(userId);
            
            if (!success)
            {
                return NotFound(new ApiResponse<RecipientProfileDto>(false, message));
            }
            
            return Ok(new ApiResponse<RecipientProfileDto>(success, message, data));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<RecipientProfileDto>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recipient profile");
            return StatusCode(500, new ApiResponse<RecipientProfileDto>(false, "An error occurred"));
        }
    }

    /// <summary>
    /// Create a new recipient profile
    /// </summary>
    [HttpPost("profile")]
    public async Task<ActionResult<ApiResponse<RecipientProfileDto>>> CreateProfile([FromBody] RecipientRegistrationDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new ApiResponse<RecipientProfileDto>(false, "Validation failed", null, errors));
            }

            var userId = GetCurrentUserId();
            var (success, message, data) = await _recipientService.CreateRecipientProfileAsync(userId, request);
            
            if (!success)
            {
                return BadRequest(new ApiResponse<RecipientProfileDto>(false, message));
            }
            
            return CreatedAtAction(nameof(GetProfile), new ApiResponse<RecipientProfileDto>(success, message, data));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<RecipientProfileDto>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recipient profile");
            return StatusCode(500, new ApiResponse<RecipientProfileDto>(false, "An error occurred"));
        }
    }

    /// <summary>
    /// Update current user's recipient profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<RecipientProfileDto>>> UpdateProfile([FromBody] RecipientRegistrationDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new ApiResponse<RecipientProfileDto>(false, "Validation failed", null, errors));
            }

            var userId = GetCurrentUserId();
            var (success, message, data) = await _recipientService.UpdateRecipientProfileAsync(userId, request);
            
            if (!success)
            {
                return NotFound(new ApiResponse<RecipientProfileDto>(false, message));
            }
            
            return Ok(new ApiResponse<RecipientProfileDto>(success, message, data));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<RecipientProfileDto>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recipient profile");
            return StatusCode(500, new ApiResponse<RecipientProfileDto>(false, "An error occurred"));
        }
    }

    #endregion

    #region Donation Requests

    /// <summary>
    /// Create a new donation request
    /// </summary>
    [HttpPost("requests")]
    public async Task<ActionResult<ApiResponse<DonationRequestDto>>> CreateDonationRequest([FromBody] CreateDonationRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new ApiResponse<DonationRequestDto>(false, "Validation failed", null, errors));
            }

            var userId = GetCurrentUserId();
            var (success, message, data) = await _recipientService.CreateDonationRequestAsync(userId, request);
            
            if (!success)
            {
                return BadRequest(new ApiResponse<DonationRequestDto>(false, message));
            }
            
            return CreatedAtAction(nameof(GetDonationRequest), new { requestId = data!.Id }, 
                new ApiResponse<DonationRequestDto>(success, message, data));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<DonationRequestDto>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating donation request");
            return StatusCode(500, new ApiResponse<DonationRequestDto>(false, "An error occurred"));
        }
    }

    /// <summary>
    /// Get all donation requests for current user
    /// </summary>
    [HttpGet("requests")]
    public async Task<ActionResult<ApiResponse<List<DonationRequestDto>>>> GetDonationRequests()
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, message, data) = await _recipientService.GetUserDonationRequestsAsync(userId);
            return Ok(new ApiResponse<List<DonationRequestDto>>(success, message, data));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<List<DonationRequestDto>>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving donation requests");
            return StatusCode(500, new ApiResponse<List<DonationRequestDto>>(false, "An error occurred"));
        }
    }

    /// <summary>
    /// Get a specific donation request by ID
    /// </summary>
    [HttpGet("requests/{requestId}")]
    public async Task<ActionResult<ApiResponse<DonationRequestDto>>> GetDonationRequest(int requestId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, message, data) = await _recipientService.GetDonationRequestByIdAsync(userId, requestId);
            
            if (!success)
            {
                return NotFound(new ApiResponse<DonationRequestDto>(false, message));
            }
            
            return Ok(new ApiResponse<DonationRequestDto>(success, message, data));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<DonationRequestDto>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving donation request");
            return StatusCode(500, new ApiResponse<DonationRequestDto>(false, "An error occurred"));
        }
    }

    /// <summary>
    /// Cancel a donation request
    /// </summary>
    [HttpDelete("requests/{requestId}")]
    public async Task<ActionResult<ApiResponse<object>>> CancelDonationRequest(int requestId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, message) = await _recipientService.CancelDonationRequestAsync(userId, requestId);
            
            if (!success)
            {
                return NotFound(new ApiResponse<object>(false, message));
            }
            
            return Ok(new ApiResponse<object>(success, message));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<object>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling donation request");
            return StatusCode(500, new ApiResponse<object>(false, "An error occurred"));
        }
    }

    /// <summary>
    /// Distribute a donation request to matching donors
    /// </summary>
    [HttpPost("requests/{requestId}/distribute")]
    public async Task<ActionResult<ApiResponse<object>>> DistributeRequest(int requestId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, message, matchedCount) = await _recipientService.DistributeRequestToDonorsAsync(userId, requestId);
            
            if (!success)
            {
                return NotFound(new ApiResponse<object>(false, message));
            }
            
            return Ok(new ApiResponse<object>(success, message, new { MatchedCount = matchedCount }));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<object>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error distributing donation request");
            return StatusCode(500, new ApiResponse<object>(false, "An error occurred"));
        }
    }

    #endregion

    #region Matching Donors

    /// <summary>
    /// Find matching donors based on blood group and donation type
    /// </summary>
    [HttpGet("donors/matching")]
    public async Task<ActionResult<ApiResponse<List<MatchingDonorDto>>>> FindMatchingDonors(
        [FromQuery] BloodGroup bloodGroup, 
        [FromQuery] DonationType donationType,
        [FromQuery] string? city = null)
    {
        try
        {
            var (success, message, data) = await _recipientService.FindMatchingDonorsAsync(bloodGroup, donationType, city);
            return Ok(new ApiResponse<List<MatchingDonorDto>>(success, message, data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching donors");
            return StatusCode(500, new ApiResponse<List<MatchingDonorDto>>(false, "An error occurred"));
        }
    }

    #endregion

    #region Donor History

    /// <summary>
    /// Get donor history for the current recipient (accepted/completed donors)
    /// </summary>
    [HttpGet("donor-history")]
    public async Task<ActionResult<ApiResponse<List<DonorHistoryDto>>>> GetDonorHistory()
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, message, data) = await _recipientService.GetDonorHistoryAsync(userId);
            return Ok(new ApiResponse<List<DonorHistoryDto>>(success, message, data));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<List<DonorHistoryDto>>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving donor history");
            return StatusCode(500, new ApiResponse<List<DonorHistoryDto>>(false, "An error occurred"));
        }
    }

    /// <summary>
    /// Get full donation history for the current recipient (all records, no deduplication)
    /// </summary>
    [HttpGet("donor-history/all")]
    public async Task<ActionResult<ApiResponse<List<DonorHistoryDto>>>> GetFullDonorHistory()
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, message, data) = await _recipientService.GetFullDonorHistoryAsync(userId);
            return Ok(new ApiResponse<List<DonorHistoryDto>>(success, message, data));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<List<DonorHistoryDto>>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving full donor history");
            return StatusCode(500, new ApiResponse<List<DonorHistoryDto>>(false, "An error occurred"));
        }
    }

    #endregion

    #region Dashboard Stats

    /// <summary>
    /// Get dashboard statistics for current recipient
    /// </summary>
    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetDashboardStats()
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, message, data) = await _recipientService.GetRecipientDashboardStatsAsync(userId);
            return Ok(new ApiResponse<object>(success, message, data));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<object>(false, "Unauthorized"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500, new ApiResponse<object>(false, "An error occurred"));
        }
    }

    #endregion
}
