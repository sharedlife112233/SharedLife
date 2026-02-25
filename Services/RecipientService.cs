using Microsoft.EntityFrameworkCore;
using SharedLife.Data;
using SharedLife.Models.DTOs.Recipient;
using SharedLife.Models.Entities;
using SharedLife.Models.Enums;
using SharedLife.Services.Interfaces;
using SharedLife.Utilities;

namespace SharedLife.Services;

public class RecipientService : IRecipientService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RecipientService> _logger;

    public RecipientService(ApplicationDbContext context, ILogger<RecipientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Recipient Profile

    public async Task<(bool Success, string Message, RecipientStatusDto? Data)> CheckRecipientStatusAsync(int userId)
    {
        try
        {
            var hasProfile = await _context.Recipients.AnyAsync(r => r.UserId == userId);
            return (true, "Status retrieved", new RecipientStatusDto { HasRecipientProfile = hasProfile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking recipient status for user {UserId}", userId);
            return (false, "An error occurred", null);
        }
    }

    public async Task<(bool Success, string Message, RecipientProfileDto? Data)> CreateRecipientProfileAsync(int userId, RecipientRegistrationDto request)
    {
        try
        {
            // Check if profile already exists
            if (await _context.Recipients.AnyAsync(r => r.UserId == userId))
            {
                return (false, "Recipient profile already exists", null);
            }

            // Get user info
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User not found", null);
            }

            // Verify user has Recipient role
            if (user.Role != UserRole.Recipient)
            {
                return (false, "User is not registered as a recipient", null);
            }

            var recipient = new Recipient
            {
                UserId = userId,
                BloodGroup = request.BloodGroup,
                MedicalCondition = request.MedicalCondition,
                HospitalName = request.HospitalName,
                HospitalAddress = request.HospitalAddress,
                City = request.City,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                EmergencyContactRelation = request.EmergencyContactRelation,
                CreatedAt = TimeHelper.Now
            };

            _context.Recipients.Add(recipient);
            await _context.SaveChangesAsync();

            var profile = await GetRecipientProfileDtoAsync(recipient, user);
            _logger.LogInformation("Recipient profile created for user {UserId}", userId);
            return (true, "Recipient profile created successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recipient profile for user {UserId}", userId);
            return (false, "An error occurred while creating the profile", null);
        }
    }

    public async Task<(bool Success, string Message, RecipientProfileDto? Data)> GetRecipientProfileAsync(int userId)
    {
        try
        {
            var recipient = await _context.Recipients
                .Include(r => r.User)
                .Include(r => r.DonationRequests)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recipient == null)
            {
                return (false, "Recipient profile not found", null);
            }

            var profile = await GetRecipientProfileDtoAsync(recipient, recipient.User);
            return (true, "Profile retrieved successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recipient profile for user {UserId}", userId);
            return (false, "An error occurred while retrieving the profile", null);
        }
    }

    public async Task<(bool Success, string Message, RecipientProfileDto? Data)> UpdateRecipientProfileAsync(int userId, RecipientRegistrationDto request)
    {
        try
        {
            var recipient = await _context.Recipients
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recipient == null)
            {
                return (false, "Recipient profile not found", null);
            }

            recipient.BloodGroup = request.BloodGroup;
            recipient.MedicalCondition = request.MedicalCondition;
            recipient.HospitalName = request.HospitalName;
            recipient.HospitalAddress = request.HospitalAddress;
            recipient.City = request.City;
            recipient.EmergencyContactName = request.EmergencyContactName;
            recipient.EmergencyContactPhone = request.EmergencyContactPhone;
            recipient.EmergencyContactRelation = request.EmergencyContactRelation;
            recipient.UpdatedAt = TimeHelper.Now;

            await _context.SaveChangesAsync();

            var profile = await GetRecipientProfileDtoAsync(recipient, recipient.User);
            _logger.LogInformation("Recipient profile updated for user {UserId}", userId);
            return (true, "Profile updated successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recipient profile for user {UserId}", userId);
            return (false, "An error occurred while updating the profile", null);
        }
    }

    #endregion

    #region Donation Requests

    public async Task<(bool Success, string Message, DonationRequestDto? Data)> CreateDonationRequestAsync(int userId, CreateDonationRequestDto request)
    {
        try
        {
            var recipient = await _context.Recipients
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recipient == null)
            {
                return (false, "Recipient profile not found. Please create a profile first.", null);
            }

            var donationRequest = new DonationRequest
            {
                RecipientId = recipient.Id,
                BloodGroup = request.BloodGroup,
                DonationType = request.DonationType,
                Quantity = request.Quantity,
                UrgencyLevel = request.UrgencyLevel,
                RequiredDateTime = request.RequiredDateTime,
                HospitalName = request.HospitalName,
                HospitalLocation = request.HospitalLocation,
                City = request.City,
                ContactName = request.ContactName,
                ContactPhone = request.ContactPhone,
                MedicalNotes = request.MedicalNotes,
                AdditionalRequirements = request.AdditionalRequirements,
                Status = RequestStatus.Pending,
                CreatedAt = TimeHelper.Now
            };

            _context.DonationRequests.Add(donationRequest);
            await _context.SaveChangesAsync();

            var dto = MapToDonationRequestDto(donationRequest, recipient.User.FullName);
            _logger.LogInformation("Donation request created by user {UserId}, Request ID: {RequestId}", userId, donationRequest.Id);
            return (true, "Donation request created successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating donation request for user {UserId}", userId);
            return (false, "An error occurred while creating the request", null);
        }
    }

    public async Task<(bool Success, string Message, List<DonationRequestDto>? Data)> GetUserDonationRequestsAsync(int userId)
    {
        try
        {
            var recipient = await _context.Recipients
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recipient == null)
            {
                return (true, "No requests found", new List<DonationRequestDto>());
            }

            var requests = await _context.DonationRequests
                .Where(dr => dr.RecipientId == recipient.Id)
                .OrderByDescending(dr => dr.CreatedAt)
                .ToListAsync();

            var dtos = requests.Select(r => MapToDonationRequestDto(r, recipient.User.FullName)).ToList();
            return (true, "Requests retrieved successfully", dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving donation requests for user {UserId}", userId);
            return (false, "An error occurred while retrieving requests", null);
        }
    }

    public async Task<(bool Success, string Message, DonationRequestDto? Data)> GetDonationRequestByIdAsync(int userId, int requestId)
    {
        try
        {
            var recipient = await _context.Recipients
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recipient == null)
            {
                return (false, "Recipient profile not found", null);
            }

            var request = await _context.DonationRequests
                .FirstOrDefaultAsync(dr => dr.Id == requestId && dr.RecipientId == recipient.Id);

            if (request == null)
            {
                return (false, "Donation request not found", null);
            }

            var dto = MapToDonationRequestDto(request, recipient.User.FullName);
            return (true, "Request retrieved successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving donation request {RequestId} for user {UserId}", requestId, userId);
            return (false, "An error occurred while retrieving the request", null);
        }
    }

    public async Task<(bool Success, string Message)> CancelDonationRequestAsync(int userId, int requestId)
    {
        try
        {
            var recipient = await _context.Recipients.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recipient == null)
            {
                return (false, "Recipient profile not found");
            }

            var request = await _context.DonationRequests
                .FirstOrDefaultAsync(dr => dr.Id == requestId && dr.RecipientId == recipient.Id);

            if (request == null)
            {
                return (false, "Donation request not found");
            }

            if (request.Status == RequestStatus.Completed)
            {
                return (false, "Cannot cancel a completed request");
            }

            request.Status = RequestStatus.Cancelled;
            request.UpdatedAt = TimeHelper.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Donation request {RequestId} cancelled by user {UserId}", requestId, userId);
            return (true, "Request cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling donation request {RequestId} for user {UserId}", requestId, userId);
            return (false, "An error occurred while cancelling the request");
        }
    }

    public async Task<(bool Success, string Message, DonationRequestDto? Data)> UpdateDonationRequestStatusAsync(int userId, int requestId, RequestStatus status)
    {
        try
        {
            var recipient = await _context.Recipients
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recipient == null)
            {
                return (false, "Recipient profile not found", null);
            }

            var request = await _context.DonationRequests
                .FirstOrDefaultAsync(dr => dr.Id == requestId && dr.RecipientId == recipient.Id);

            if (request == null)
            {
                return (false, "Donation request not found", null);
            }

            request.Status = status;
            request.UpdatedAt = TimeHelper.Now;
            
            if (status == RequestStatus.Completed)
            {
                request.CompletedAt = TimeHelper.Now;
            }

            await _context.SaveChangesAsync();

            var dto = MapToDonationRequestDto(request, recipient.User.FullName);
            return (true, "Status updated successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating donation request status for request {RequestId}", requestId);
            return (false, "An error occurred while updating the status", null);
        }
    }

    #endregion

    #region Matching Donors

    public async Task<(bool Success, string Message, List<MatchingDonorDto>? Data)> FindMatchingDonorsAsync(BloodGroup bloodGroup, DonationType donationType, string? city = null)
    {
        try
        {
            // Get compatible blood groups
            var compatibleGroups = GetCompatibleBloodGroups(bloodGroup);

            var query = _context.Donors
                .Include(d => d.User)
                .Where(d => d.IsAvailable && 
                           d.Status == DonorStatus.Active &&
                           compatibleGroups.Contains(d.BloodGroup));

            // Filter by donation type preferences
            switch (donationType)
            {
                case DonationType.Blood:
                    query = query.Where(d => CanDonateBlood(d.LastBloodDonationDate));
                    break;
                case DonationType.Plasma:
                    query = query.Where(d => d.WillingToDonatePlasma);
                    break;
                case DonationType.Platelets:
                    query = query.Where(d => d.WillingToDonatePlatelets);
                    break;
                case DonationType.Organ:
                    query = query.Where(d => d.WillingToDonateOrgan);
                    break;
                case DonationType.BoneMarrow:
                    query = query.Where(d => d.WillingToDonateBoneMarrow);
                    break;
                case DonationType.Eye:
                    query = query.Where(d => d.WillingToDonateEye);
                    break;
            }

            // Filter by city if provided
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(d => d.User.Address != null && d.User.Address.Contains(city));
            }

            var donors = await query.ToListAsync();

            var dtos = donors.Select(d => new MatchingDonorDto
            {
                DonorId = d.Id,
                FullName = d.User.FullName,
                BloodGroup = d.BloodGroup,
                BloodGroupDisplay = GetBloodGroupDisplay(d.BloodGroup),
                City = ExtractCity(d.User.Address),
                IsAvailable = d.IsAvailable,
                Status = d.Status,
                StatusDisplay = d.Status.ToString(),
                WillingToDonatePlasma = d.WillingToDonatePlasma,
                WillingToDonatePlatelets = d.WillingToDonatePlatelets,
                WillingToDonateOrgan = d.WillingToDonateOrgan,
                WillingToDonateBoneMarrow = d.WillingToDonateBoneMarrow,
                WillingToDonateEye = d.WillingToDonateEye,
                CanDonateBlood = CanDonateBlood(d.LastBloodDonationDate),
                DaysUntilCanDonate = CalculateDaysUntilCanDonate(d.LastBloodDonationDate),
                TotalBloodDonations = d.TotalBloodDonations
            }).ToList();

            return (true, $"Found {dtos.Count} matching donors", dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching donors for blood group {BloodGroup}", bloodGroup);
            return (false, "An error occurred while finding matching donors", null);
        }
    }

    public async Task<(bool Success, string Message, int MatchedCount)> DistributeRequestToDonorsAsync(int userId, int requestId)
    {
        try
        {
            var recipient = await _context.Recipients.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recipient == null)
            {
                return (false, "Recipient profile not found", 0);
            }

            var request = await _context.DonationRequests
                .FirstOrDefaultAsync(dr => dr.Id == requestId && dr.RecipientId == recipient.Id);

            if (request == null)
            {
                return (false, "Donation request not found", 0);
            }

            // Find matching donors
            var (success, _, donors) = await FindMatchingDonorsAsync(request.BloodGroup, request.DonationType, request.City);
            if (!success || donors == null || donors.Count == 0)
            {
                return (true, "No matching donors found", 0);
            }

            // Create DonorRequest entries for each matching donor
            var existingDonorIds = await _context.DonorRequests
                .Where(dr => dr.DonationRequestId == requestId)
                .Select(dr => dr.DonorId)
                .ToListAsync();

            var newDonorRequests = donors
                .Where(d => !existingDonorIds.Contains(d.DonorId))
                .Select(d => new DonorRequest
                {
                    DonationRequestId = requestId,
                    DonorId = d.DonorId,
                    Status = RequestStatus.Sent,
                    IsNotified = true,
                    NotifiedAt = TimeHelper.Now,
                    CreatedAt = TimeHelper.Now
                }).ToList();

            if (newDonorRequests.Any())
            {
                _context.DonorRequests.AddRange(newDonorRequests);
                
                request.Status = RequestStatus.Sent;
                request.MatchedDonorsCount = donors.Count;
                request.UpdatedAt = TimeHelper.Now;
                
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Distributed request {RequestId} to {Count} donors", requestId, newDonorRequests.Count);
            return (true, $"Request sent to {newDonorRequests.Count} matching donors", newDonorRequests.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error distributing request {RequestId} to donors", requestId);
            return (false, "An error occurred while distributing the request", 0);
        }
    }

    #endregion

    #region Stats

    public async Task<(bool Success, string Message, object? Data)> GetRecipientDashboardStatsAsync(int userId)
    {
        try
        {
            var recipient = await _context.Recipients
                .Include(r => r.DonationRequests)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recipient == null)
            {
                return (true, "Stats retrieved", new
                {
                    HasProfile = false,
                    TotalRequests = 0,
                    ActiveRequests = 0,
                    CompletedRequests = 0,
                    PendingRequests = 0,
                    MatchedDonors = 0
                });
            }

            var requests = recipient.DonationRequests;
            var stats = new
            {
                HasProfile = true,
                TotalRequests = requests.Count,
                ActiveRequests = requests.Count(r => r.Status == RequestStatus.Sent || r.Status == RequestStatus.Accepted),
                CompletedRequests = requests.Count(r => r.Status == RequestStatus.Completed),
                PendingRequests = requests.Count(r => r.Status == RequestStatus.Pending),
                MatchedDonors = requests.Sum(r => r.MatchedDonorsCount),
                AcceptedDonors = requests.Sum(r => r.AcceptedDonorsCount)
            };

            return (true, "Stats retrieved successfully", stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats for user {UserId}", userId);
            return (false, "An error occurred while retrieving stats", null);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<RecipientProfileDto> GetRecipientProfileDtoAsync(Recipient recipient, User user)
    {
        var requests = await _context.DonationRequests
            .Where(dr => dr.RecipientId == recipient.Id)
            .ToListAsync();

        return new RecipientProfileDto
        {
            Id = recipient.Id,
            UserId = recipient.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            DateOfBirth = user.DateOfBirth,
            BloodGroup = recipient.BloodGroup,
            BloodGroupDisplay = GetBloodGroupDisplay(recipient.BloodGroup),
            MedicalCondition = recipient.MedicalCondition,
            HospitalName = recipient.HospitalName,
            HospitalAddress = recipient.HospitalAddress,
            City = recipient.City,
            EmergencyContactName = recipient.EmergencyContactName,
            EmergencyContactPhone = recipient.EmergencyContactPhone,
            EmergencyContactRelation = recipient.EmergencyContactRelation,
            IsVerified = recipient.IsVerified,
            VerifiedAt = recipient.VerifiedAt,
            CreatedAt = recipient.CreatedAt,
            UpdatedAt = recipient.UpdatedAt,
            TotalRequests = requests.Count,
            ActiveRequests = requests.Count(r => r.Status == RequestStatus.Sent || r.Status == RequestStatus.Accepted),
            CompletedRequests = requests.Count(r => r.Status == RequestStatus.Completed)
        };
    }

    private DonationRequestDto MapToDonationRequestDto(DonationRequest request, string recipientName)
    {
        return new DonationRequestDto
        {
            Id = request.Id,
            RecipientId = request.RecipientId,
            RecipientName = recipientName,
            BloodGroup = request.BloodGroup,
            BloodGroupDisplay = GetBloodGroupDisplay(request.BloodGroup),
            DonationType = request.DonationType,
            DonationTypeDisplay = request.DonationType.ToString(),
            Quantity = request.Quantity,
            UrgencyLevel = request.UrgencyLevel,
            UrgencyLevelDisplay = request.UrgencyLevel.ToString(),
            RequiredDateTime = request.RequiredDateTime,
            HospitalName = request.HospitalName,
            HospitalLocation = request.HospitalLocation,
            City = request.City,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone,
            MedicalNotes = request.MedicalNotes,
            AdditionalRequirements = request.AdditionalRequirements,
            Status = request.Status,
            StatusDisplay = request.Status.ToString(),
            MatchedDonorsCount = request.MatchedDonorsCount,
            AcceptedDonorsCount = request.AcceptedDonorsCount,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            CompletedAt = request.CompletedAt
        };
    }

    private static string GetBloodGroupDisplay(BloodGroup bloodGroup)
    {
        return bloodGroup switch
        {
            BloodGroup.APositive => "A+",
            BloodGroup.ANegative => "A-",
            BloodGroup.BPositive => "B+",
            BloodGroup.BNegative => "B-",
            BloodGroup.ABPositive => "AB+",
            BloodGroup.ABNegative => "AB-",
            BloodGroup.OPositive => "O+",
            BloodGroup.ONegative => "O-",
            _ => bloodGroup.ToString()
        };
    }

    private static List<BloodGroup> GetCompatibleBloodGroups(BloodGroup recipientBloodGroup)
    {
        // Blood compatibility chart - who can donate to whom
        return recipientBloodGroup switch
        {
            BloodGroup.ABPositive => new List<BloodGroup> 
            { 
                BloodGroup.ABPositive, BloodGroup.ABNegative, 
                BloodGroup.APositive, BloodGroup.ANegative,
                BloodGroup.BPositive, BloodGroup.BNegative,
                BloodGroup.OPositive, BloodGroup.ONegative 
            },
            BloodGroup.ABNegative => new List<BloodGroup> 
            { 
                BloodGroup.ABNegative, BloodGroup.ANegative, 
                BloodGroup.BNegative, BloodGroup.ONegative 
            },
            BloodGroup.APositive => new List<BloodGroup> 
            { 
                BloodGroup.APositive, BloodGroup.ANegative, 
                BloodGroup.OPositive, BloodGroup.ONegative 
            },
            BloodGroup.ANegative => new List<BloodGroup> 
            { 
                BloodGroup.ANegative, BloodGroup.ONegative 
            },
            BloodGroup.BPositive => new List<BloodGroup> 
            { 
                BloodGroup.BPositive, BloodGroup.BNegative, 
                BloodGroup.OPositive, BloodGroup.ONegative 
            },
            BloodGroup.BNegative => new List<BloodGroup> 
            { 
                BloodGroup.BNegative, BloodGroup.ONegative 
            },
            BloodGroup.OPositive => new List<BloodGroup> 
            { 
                BloodGroup.OPositive, BloodGroup.ONegative 
            },
            BloodGroup.ONegative => new List<BloodGroup> 
            { 
                BloodGroup.ONegative 
            },
            _ => new List<BloodGroup> { recipientBloodGroup }
        };
    }

    private static bool CanDonateBlood(DateTime? lastDonation)
    {
        if (!lastDonation.HasValue) return true;
        return (TimeHelper.Now - lastDonation.Value).TotalDays >= 56; // 8 weeks
    }

    private static int? CalculateDaysUntilCanDonate(DateTime? lastDonation)
    {
        if (!lastDonation.HasValue) return 0;
        var daysSince = (TimeHelper.Now - lastDonation.Value).TotalDays;
        var daysRemaining = 56 - (int)daysSince;
        return daysRemaining > 0 ? daysRemaining : 0;
    }

    private static string? ExtractCity(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;
        // Simple extraction - take the last part after comma
        var parts = address.Split(',');
        return parts.Length > 0 ? parts[^1].Trim() : address;
    }

    #endregion
}
