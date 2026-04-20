using Microsoft.EntityFrameworkCore;
using SharedLife.Data;
using SharedLife.Models.DTOs.Hospital;
using SharedLife.Models.DTOs.Recipient;
using SharedLife.Models.Entities;
using SharedLife.Models.Enums;
using SharedLife.Services.Interfaces;
using SharedLife.Utilities;

namespace SharedLife.Services;

public class HospitalService : IHospitalService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HospitalService> _logger;

    public HospitalService(ApplicationDbContext context, ILogger<HospitalService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Hospital Profile Management

    public async Task<(bool Success, string Message, HospitalProfileDto? Data)> RegisterHospitalAsync(int userId, HospitalRegistrationDto request)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User not found", null);
            }

            if (user.Role != UserRole.Hospital)
            {
                return (false, "Only users with Hospital role can register as hospitals", null);
            }

            var existingHospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.UserId == userId);
            if (existingHospital != null)
            {
                return (false, "Hospital profile already exists", null);
            }

            // Check for duplicate registration number
            var duplicateRegNumber = await _context.Hospitals.AnyAsync(h => h.RegistrationNumber == request.RegistrationNumber);
            if (duplicateRegNumber)
            {
                return (false, "A hospital with this registration number already exists", null);
            }

            var hospital = new Hospital
            {
                UserId = userId,
                HospitalName = request.HospitalName,
                RegistrationNumber = request.RegistrationNumber,
                HospitalType = request.HospitalType,
                Address = request.Address,
                City = request.City,
                State = request.State,
                PinCode = request.PinCode,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                AlternatePhone = request.AlternatePhone,
                Website = request.Website,
                HasBloodBank = request.HasBloodBank,
                HasOrganTransplant = request.HasOrganTransplant,
                HasEyeBank = request.HasEyeBank,
                HasBoneMarrowRegistry = request.HasBoneMarrowRegistry,
                BloodBankCapacity = request.BloodBankCapacity,
                BedCapacity = request.BedCapacity,
                OperatingHours = request.OperatingHours,
                IsOpen24x7 = request.IsOpen24x7,
                IsVerified = false,
                IsActive = true,
                CreatedAt = TimeHelper.Now
            };

            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();

            var profile = MapToProfileDto(hospital, user);
            _logger.LogInformation("Hospital profile created for user {UserId}: {HospitalName}", userId, hospital.HospitalName);

            return (true, "Hospital profile created successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating hospital profile for user {UserId}", userId);
            return (false, "An error occurred while creating hospital profile", null);
        }
    }

    public async Task<(bool Success, string Message, HospitalProfileDto? Data)> GetHospitalProfileAsync(int userId)
    {
        try
        {
            var hospital = await _context.Hospitals
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hospital == null)
            {
                return (false, "Hospital profile not found", null);
            }

            var profile = MapToProfileDto(hospital, hospital.User);
            return (true, "Hospital profile retrieved successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hospital profile for user {UserId}", userId);
            return (false, "An error occurred while retrieving hospital profile", null);
        }
    }

    public async Task<(bool Success, string Message, HospitalProfileDto? Data)> GetHospitalByIdAsync(int hospitalId)
    {
        try
        {
            var hospital = await _context.Hospitals
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.Id == hospitalId);

            if (hospital == null)
            {
                return (false, "Hospital not found", null);
            }

            var profile = MapToProfileDto(hospital, hospital.User);
            return (true, "Hospital retrieved successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hospital by ID {HospitalId}", hospitalId);
            return (false, "An error occurred while retrieving hospital", null);
        }
    }

    public async Task<(bool Success, string Message, HospitalProfileDto? Data)> UpdateHospitalAsync(int userId, HospitalUpdateDto request)
    {
        try
        {
            var hospital = await _context.Hospitals
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hospital == null)
            {
                return (false, "Hospital profile not found", null);
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.HospitalName)) hospital.HospitalName = request.HospitalName;
            if (!string.IsNullOrEmpty(request.Address)) hospital.Address = request.Address;
            if (!string.IsNullOrEmpty(request.City)) hospital.City = request.City;
            if (!string.IsNullOrEmpty(request.State)) hospital.State = request.State;
            if (!string.IsNullOrEmpty(request.PinCode)) hospital.PinCode = request.PinCode;
            if (!string.IsNullOrEmpty(request.ContactEmail)) hospital.ContactEmail = request.ContactEmail;
            if (!string.IsNullOrEmpty(request.ContactPhone)) hospital.ContactPhone = request.ContactPhone;
            if (request.AlternatePhone != null) hospital.AlternatePhone = request.AlternatePhone;
            if (request.Website != null) hospital.Website = request.Website;
            if (request.HasBloodBank.HasValue) hospital.HasBloodBank = request.HasBloodBank.Value;
            if (request.HasOrganTransplant.HasValue) hospital.HasOrganTransplant = request.HasOrganTransplant.Value;
            if (request.HasEyeBank.HasValue) hospital.HasEyeBank = request.HasEyeBank.Value;
            if (request.HasBoneMarrowRegistry.HasValue) hospital.HasBoneMarrowRegistry = request.HasBoneMarrowRegistry.Value;
            if (request.BloodBankCapacity.HasValue) hospital.BloodBankCapacity = request.BloodBankCapacity;
            if (request.BedCapacity.HasValue) hospital.BedCapacity = request.BedCapacity;
            if (request.OperatingHours != null) hospital.OperatingHours = request.OperatingHours;
            if (request.IsOpen24x7.HasValue) hospital.IsOpen24x7 = request.IsOpen24x7.Value;

            hospital.UpdatedAt = TimeHelper.Now;
            await _context.SaveChangesAsync();

            var profile = MapToProfileDto(hospital, hospital.User);
            _logger.LogInformation("Hospital profile updated for user {UserId}", userId);

            return (true, "Hospital profile updated successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hospital profile for user {UserId}", userId);
            return (false, "An error occurred while updating hospital profile", null);
        }
    }

    public async Task<bool> IsHospitalAsync(int userId)
    {
        return await _context.Hospitals.AnyAsync(h => h.UserId == userId);
    }

    #endregion

    #region Dashboard

    public async Task<(bool Success, string Message, HospitalDashboardDto? Data)> GetDashboardAsync(int userId)
    {
        try
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.UserId == userId);
            if (hospital == null)
            {
                return (false, "Hospital profile not found", null);
            }

            var now = TimeHelper.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // Get pending donors for verification
            var pendingCount = await _context.Donors
                .Where(d => d.Status == DonorStatus.Pending)
                .CountAsync();

            // Get donors verified by this hospital
            var verifiedByHospital = await _context.Donors
                .Where(d => d.VerifiedByHospitalId == hospital.Id)
                .CountAsync();

            var verifiedThisMonth = await _context.Donors
                .Where(d => d.VerifiedByHospitalId == hospital.Id && d.VerifiedAt >= startOfMonth)
                .CountAsync();

            // Get requests in hospital's city
            var pendingRequests = await _context.DonationRequests
                .Where(r => r.City == hospital.City && r.Status == RequestStatus.Pending)
                .CountAsync();

            var activeRequests = await _context.DonationRequests
                .Where(r => r.City == hospital.City && (r.Status == RequestStatus.Sent || r.Status == RequestStatus.Accepted))
                .CountAsync();

            var completedRequests = await _context.DonationRequests
                .Where(r => r.City == hospital.City && r.Status == RequestStatus.Completed)
                .CountAsync();

            // Get donor counts by blood group
            var donorsByBloodGroup = await _context.Donors
                .Where(d => d.Status == DonorStatus.Verified)
                .GroupBy(d => d.BloodGroup)
                .Select(g => new { BloodGroup = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.BloodGroup, x => x.Count);

            // Get recent verifications
            var recentVerifications = await _context.Donors
                .Where(d => d.VerifiedByHospitalId == hospital.Id)
                .OrderByDescending(d => d.VerifiedAt)
                .Take(5)
                .Include(d => d.User)
                .Select(d => new RecentVerificationDto
                {
                    DonorId = d.Id,
                    DonorName = d.User.FullName,
                    BloodGroup = d.BloodGroup.ToString(),
                    Status = d.Status.ToString(),
                    VerifiedAt = d.VerifiedAt ?? TimeHelper.Now
                })
                .ToListAsync();

            // Get recent requests in the area
            var recentRequests = await _context.DonationRequests
                .Where(r => r.City == hospital.City)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Include(r => r.Recipient)
                    .ThenInclude(rec => rec.User)
                .Select(r => new RecentRequestActivityDto
                {
                    RequestId = r.Id,
                    RecipientName = r.Recipient.User.FullName,
                    BloodGroup = r.BloodGroup.ToString(),
                    DonationType = r.DonationType.ToString(),
                    UrgencyLevel = r.UrgencyLevel.ToString(),
                    Status = r.Status.ToString(),
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var dashboard = new HospitalDashboardDto
            {
                PendingVerifications = pendingCount,
                TotalDonorsVerified = verifiedByHospital,
                VerifiedThisMonth = verifiedThisMonth,
                PendingRequests = pendingRequests,
                ActiveRequests = activeRequests,
                CompletedRequests = completedRequests,
                TotalRequestsProcessed = hospital.TotalRequestsProcessed,
                DonorsByBloodGroup = donorsByBloodGroup,
                RecentVerifications = recentVerifications,
                RecentRequests = recentRequests
            };

            return (true, "Dashboard data retrieved successfully", dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard for user {UserId}", userId);
            return (false, "An error occurred while retrieving dashboard data", null);
        }
    }

    #endregion

    #region Donor Verification

    public async Task<(bool Success, string Message, List<PendingDonorDto>? Data)> GetPendingDonorsAsync(int userId, int page, int pageSize)
    {
        try
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.UserId == userId);
            if (hospital == null)
            {
                return (false, "Hospital profile not found", null);
            }

            var donors = await _context.Donors
                .Where(d => d.Status == DonorStatus.Pending)
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.User)
                .Select(d => MapToPendingDonorDto(d))
                .ToListAsync();

            return (true, "Pending donors retrieved successfully", donors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending donors for user {UserId}", userId);
            return (false, "An error occurred while retrieving pending donors", null);
        }
    }

    public async Task<(bool Success, string Message)> VerifyDonorAsync(int userId, DonorVerificationDto request)
    {
        try
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.UserId == userId);
            if (hospital == null)
            {
                return (false, "Hospital profile not found");
            }

            if (!hospital.IsVerified)
            {
                return (false, "Hospital must be verified to verify donors");
            }

            var donor = await _context.Donors.FindAsync(request.DonorId);
            if (donor == null)
            {
                return (false, "Donor not found");
            }

            donor.Status = request.NewStatus;
            if (request.NewStatus == DonorStatus.Verified)
            {
                donor.Status = DonorStatus.Active;
                donor.IsAvailable = true;
            }
            donor.VerifiedByHospitalId = hospital.Id;
            donor.VerifiedAt = TimeHelper.Now;
            donor.UpdatedAt = TimeHelper.Now;

            hospital.TotalDonorsVerified++;
            hospital.UpdatedAt = TimeHelper.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Donor {DonorId} verified by hospital {HospitalId} with status {Status}", 
                request.DonorId, hospital.Id, request.NewStatus);

            return (true, $"Donor verification updated to {request.NewStatus}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying donor {DonorId}", request.DonorId);
            return (false, "An error occurred while verifying donor");
        }
    }

    public async Task<(bool Success, string Message, List<PendingDonorDto>? Data)> GetVerifiedDonorsAsync(int userId, int page, int pageSize)
    {
        try
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.UserId == userId);
            if (hospital == null)
            {
                return (false, "Hospital profile not found", null);
            }

            var donors = await _context.Donors
                .Where(d => d.VerifiedByHospitalId == hospital.Id)
                .OrderByDescending(d => d.VerifiedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.User)
                .Select(d => MapToPendingDonorDto(d))
                .ToListAsync();

            return (true, "Verified donors retrieved successfully", donors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verified donors for user {UserId}", userId);
            return (false, "An error occurred while retrieving verified donors", null);
        }
    }

    #endregion

    #region Donation Requests

    public async Task<(bool Success, string Message, List<DonationRequestDto>? Data)> GetAreaRequestsAsync(int userId, int page, int pageSize, string? status)
    {
        try
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.UserId == userId);
            if (hospital == null)
            {
                return (false, "Hospital profile not found", null);
            }

            var query = _context.DonationRequests
                .Where(r => r.City == hospital.City)
                .Include(r => r.Recipient)
                    .ThenInclude(rec => rec.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RequestStatus>(status, out var requestStatus))
            {
                query = query.Where(r => r.Status == requestStatus);
            }

            var requests = await query
                .OrderByDescending(r => r.UrgencyLevel)
                .ThenByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new DonationRequestDto
                {
                    Id = r.Id,
                    RecipientId = r.RecipientId,
                    RecipientName = r.Recipient.User.FullName,
                    BloodGroup = r.BloodGroup,
                    BloodGroupDisplay = r.BloodGroup.ToString(),
                    DonationType = r.DonationType,
                    DonationTypeDisplay = r.DonationType.ToString(),
                    UrgencyLevel = r.UrgencyLevel,
                    UrgencyLevelDisplay = r.UrgencyLevel.ToString(),
                    Quantity = r.Quantity,
                    HospitalName = r.HospitalName,
                    HospitalLocation = r.HospitalLocation,
                    City = r.City,
                    ContactName = r.ContactName,
                    ContactPhone = r.ContactPhone,
                    MedicalNotes = r.MedicalNotes,
                    AdditionalRequirements = r.AdditionalRequirements,
                    RequiredDateTime = r.RequiredDateTime,
                    Status = r.Status,
                    StatusDisplay = r.Status.ToString(),
                    MatchedDonorsCount = r.MatchedDonorsCount,
                    AcceptedDonorsCount = r.AcceptedDonorsCount,
                    CreatedAt = r.CreatedAt,
                    CompletedAt = r.CompletedAt
                })
                .ToListAsync();

            return (true, "Requests retrieved successfully", requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting area requests for user {UserId}", userId);
            return (false, "An error occurred while retrieving requests", null);
        }
    }

    public async Task<(bool Success, string Message)> ProcessRequestAsync(int userId, int requestId, string action, string? notes)
    {
        try
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.UserId == userId);
            if (hospital == null)
            {
                return (false, "Hospital profile not found");
            }

            var request = await _context.DonationRequests.FindAsync(requestId);
            if (request == null)
            {
                return (false, "Request not found");
            }

            switch (action.ToLower())
            {
                case "approve":
                    request.Status = RequestStatus.Sent;
                    break;
                case "complete":
                    request.Status = RequestStatus.Completed;
                    request.CompletedAt = TimeHelper.Now;
                    hospital.TotalRequestsProcessed++;
                    break;
                case "cancel":
                    request.Status = RequestStatus.Cancelled;
                    break;
                default:
                    return (false, "Invalid action");
            }

            request.UpdatedAt = TimeHelper.Now;
            hospital.UpdatedAt = TimeHelper.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Request {RequestId} processed by hospital {HospitalId} with action {Action}", 
                requestId, hospital.Id, action);

            return (true, $"Request {action} successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request {RequestId}", requestId);
            return (false, "An error occurred while processing request");
        }
    }

    #endregion

    #region Private Helpers

    private static HospitalProfileDto MapToProfileDto(Hospital hospital, User user)
    {
        return new HospitalProfileDto
        {
            Id = hospital.Id,
            UserId = hospital.UserId,
            UserEmail = user.Email,
            UserFullName = user.FullName,
            UserPhone = user.PhoneNumber ?? string.Empty,
            HospitalName = hospital.HospitalName,
            RegistrationNumber = hospital.RegistrationNumber,
            HospitalType = hospital.HospitalType,
            Address = hospital.Address,
            City = hospital.City,
            State = hospital.State,
            PinCode = hospital.PinCode,
            ContactEmail = hospital.ContactEmail,
            ContactPhone = hospital.ContactPhone,
            AlternatePhone = hospital.AlternatePhone,
            Website = hospital.Website,
            HasBloodBank = hospital.HasBloodBank,
            HasOrganTransplant = hospital.HasOrganTransplant,
            HasEyeBank = hospital.HasEyeBank,
            HasBoneMarrowRegistry = hospital.HasBoneMarrowRegistry,
            BloodBankCapacity = hospital.BloodBankCapacity,
            BedCapacity = hospital.BedCapacity,
            OperatingHours = hospital.OperatingHours,
            IsOpen24x7 = hospital.IsOpen24x7,
            IsVerified = hospital.IsVerified,
            VerifiedAt = hospital.VerifiedAt,
            VerificationNotes = hospital.VerificationNotes,
            IsActive = hospital.IsActive,
            TotalDonorsVerified = hospital.TotalDonorsVerified,
            TotalRequestsProcessed = hospital.TotalRequestsProcessed,
            CreatedAt = hospital.CreatedAt,
            UpdatedAt = hospital.UpdatedAt
        };
    }

    private static PendingDonorDto MapToPendingDonorDto(Donor donor)
    {
        return new PendingDonorDto
        {
            Id = donor.Id,
            UserId = donor.UserId,
            FullName = donor.User.FullName,
            Email = donor.User.Email,
            PhoneNumber = donor.User.PhoneNumber ?? string.Empty,
            Address = donor.User.Address,
            DateOfBirth = donor.User.DateOfBirth,
            BloodGroup = donor.BloodGroup.ToString(),
            Weight = donor.Weight,
            Height = donor.Height,
            WillingToDonateBlood = donor.WillingToDonateBlood,
            WillingToDonatePlasma = donor.WillingToDonatePlasma,
            WillingToDonatePlatelets = donor.WillingToDonatePlatelets,
            WillingToDonateOrgan = donor.WillingToDonateOrgan,
            WillingToDonateBoneMarrow = donor.WillingToDonateBoneMarrow,
            WillingToDonateEye = donor.WillingToDonateEye,
            PledgedOrgans = donor.PledgedOrgans,
            HasChronicDisease = donor.HasChronicDisease,
            ChronicDiseaseDetails = donor.ChronicDiseaseDetails,
            HasInfectiousDisease = donor.HasInfectiousDisease,
            IsSmoker = donor.IsSmoker,
            ConsumesAlcohol = donor.ConsumesAlcohol,
            Medications = donor.Medications,
            Allergies = donor.Allergies,
            EmergencyContactName = donor.EmergencyContactName,
            EmergencyContactPhone = donor.EmergencyContactPhone,
            EmergencyContactRelation = donor.EmergencyContactRelation,
            Status = donor.Status,
            CreatedAt = donor.CreatedAt,
            DocumentPath = donor.DocumentPath,
            DocumentOriginalName = donor.DocumentOriginalName,
            DocumentUploadedAt = donor.DocumentUploadedAt
        };
    }

    #endregion

    #region Donor Offers

    public async Task<(bool Success, string Message, List<Models.DTOs.Donor.DonorOfferDto>? Data)> GetAreaDonorOffersAsync(int userId, int page, int pageSize)
    {
        try
        {
            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.UserId == userId);
            if (hospital == null)
                return (false, "Hospital profile not found", null);

            var hospitalCity = hospital.City.Trim().ToLower();
            var offers = await _context.DonorOffers
                .Where(o => o.City.Trim().ToLower() == hospitalCity && o.Status == DonorOfferStatus.Available)
                .Include(o => o.Donor)
                    .ThenInclude(d => d.User)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new Models.DTOs.Donor.DonorOfferDto
                {
                    Id = o.Id,
                    DonorName = o.Donor.User.FullName,
                    BloodGroup = o.Donor.BloodGroup,
                    BloodGroupDisplay = o.Donor.BloodGroup.ToString(),
                    DonationType = o.DonationType,
                    DonationTypeDisplay = o.DonationType.ToString(),
                    Quantity = o.Quantity,
                    HospitalName = o.HospitalName,
                    HospitalLocation = o.HospitalLocation,
                    City = o.City,
                    PreferredDate = o.PreferredDate,
                    Notes = o.Notes,
                    Status = o.Status,
                    StatusDisplay = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return (true, "Donor offers retrieved successfully", offers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting area donor offers for user {UserId}", userId);
            return (false, "An error occurred while retrieving donor offers", null);
        }
    }

    #endregion
}
