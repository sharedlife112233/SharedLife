using Microsoft.EntityFrameworkCore;
using SharedLife.Data;
using SharedLife.Models.DTOs.Donor;
using SharedLife.Models.DTOs.Recipient;
using SharedLife.Models.Entities;
using SharedLife.Models.Enums;
using SharedLife.Services.Interfaces;
using SharedLife.Utilities;

namespace SharedLife.Services;

public class DonorService : IDonorService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DonorService> _logger;
    private const int BloodDonationCooldownDays = 56; // 8 weeks between donations

    public DonorService(ApplicationDbContext context, ILogger<DonorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, DonorProfileDto? Data)> RegisterDonorAsync(int userId, DonorRegistrationDto request)
    {
        try
        {
            // Check if user exists and is a donor
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User not found", null);
            }

            if (user.Role != UserRole.Donor)
            {
                return (false, "Only users with Donor role can register as donors", null);
            }

            // Check if donor profile already exists
            var existingDonor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (existingDonor != null)
            {
                return (false, "Donor profile already exists", null);
            }

            // Create donor profile
            var donor = new Donor
            {
                UserId = userId,
                BloodGroup = request.BloodGroup,
                Weight = request.Weight,
                Height = request.Height,
                WillingToDonatePlasma = request.WillingToDonatePlasma,
                WillingToDonatePlatelets = request.WillingToDonatePlatelets,
                WillingToDonateOrgan = request.WillingToDonateOrgan,
                WillingToDonateBoneMarrow = request.WillingToDonateBoneMarrow,
                WillingToDonateEye = request.WillingToDonateEye,
                PledgedOrgans = request.PledgedOrgans != null ? string.Join(",", request.PledgedOrgans.Select(o => (int)o)) : null,
                HasChronicDisease = request.HasChronicDisease,
                ChronicDiseaseDetails = request.ChronicDiseaseDetails,
                HasInfectiousDisease = request.HasInfectiousDisease,
                IsSmoker = request.IsSmoker,
                ConsumesAlcohol = request.ConsumesAlcohol,
                Medications = request.Medications,
                Allergies = request.Allergies,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                EmergencyContactRelation = request.EmergencyContactRelation,
                Status = DonorStatus.Pending,
                IsAvailable = true,
                CreatedAt = TimeHelper.Now
            };

            // Update user's blood group if not set
            if (user.BloodGroup == null)
            {
                user.BloodGroup = request.BloodGroup;
                user.UpdatedAt = TimeHelper.Now;
            }

            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            var profile = await GetDonorProfileDtoAsync(donor, user);
            _logger.LogInformation("Donor profile created for user {UserId}", userId);
            
            return (true, "Donor profile created successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating donor profile for user {UserId}", userId);
            return (false, "An error occurred while creating donor profile", null);
        }
    }

    public async Task<(bool Success, string Message, DonorProfileDto? Data)> GetDonorProfileAsync(int userId)
    {
        try
        {
            var donor = await _context.Donors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (donor == null)
            {
                return (false, "Donor profile not found", null);
            }

            var profile = await GetDonorProfileDtoAsync(donor, donor.User);
            return (true, "Donor profile retrieved successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving donor profile for user {UserId}", userId);
            return (false, "An error occurred while retrieving donor profile", null);
        }
    }

    public async Task<(bool Success, string Message, DonorProfileDto? Data)> GetDonorByIdAsync(int donorId)
    {
        try
        {
            var donor = await _context.Donors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == donorId);

            if (donor == null)
            {
                return (false, "Donor not found", null);
            }

            var profile = await GetDonorProfileDtoAsync(donor, donor.User);
            return (true, "Donor retrieved successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving donor {DonorId}", donorId);
            return (false, "An error occurred while retrieving donor", null);
        }
    }

    public async Task<(bool Success, string Message, DonorProfileDto? Data)> UpdateDonorAsync(int userId, DonorUpdateDto request)
    {
        try
        {
            var donor = await _context.Donors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (donor == null)
            {
                return (false, "Donor profile not found", null);
            }

            // Update donor fields
            donor.Weight = request.Weight;
            donor.Height = request.Height;
            donor.WillingToDonatePlasma = request.WillingToDonatePlasma;
            donor.WillingToDonatePlatelets = request.WillingToDonatePlatelets;
            donor.WillingToDonateOrgan = request.WillingToDonateOrgan;
            donor.WillingToDonateBoneMarrow = request.WillingToDonateBoneMarrow;
            donor.WillingToDonateEye = request.WillingToDonateEye;
            donor.PledgedOrgans = request.PledgedOrgans != null ? string.Join(",", request.PledgedOrgans.Select(o => (int)o)) : null;
            donor.HasChronicDisease = request.HasChronicDisease;
            donor.ChronicDiseaseDetails = request.ChronicDiseaseDetails;
            donor.HasInfectiousDisease = request.HasInfectiousDisease;
            donor.IsSmoker = request.IsSmoker;
            donor.ConsumesAlcohol = request.ConsumesAlcohol;
            donor.Medications = request.Medications;
            donor.Allergies = request.Allergies;
            donor.IsAvailable = request.IsAvailable;
            donor.AvailabilityNotes = request.AvailabilityNotes;
            donor.EmergencyContactName = request.EmergencyContactName;
            donor.EmergencyContactPhone = request.EmergencyContactPhone;
            donor.EmergencyContactRelation = request.EmergencyContactRelation;
            donor.UpdatedAt = TimeHelper.Now;

            await _context.SaveChangesAsync();

            var profile = await GetDonorProfileDtoAsync(donor, donor.User);
            _logger.LogInformation("Donor profile updated for user {UserId}", userId);
            
            return (true, "Donor profile updated successfully", profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating donor profile for user {UserId}", userId);
            return (false, "An error occurred while updating donor profile", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateAvailabilityAsync(int userId, DonorAvailabilityDto request)
    {
        try
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);

            if (donor == null)
            {
                return (false, "Donor profile not found");
            }

            donor.IsAvailable = request.IsAvailable;
            donor.AvailabilityNotes = request.AvailabilityNotes;
            donor.UpdatedAt = TimeHelper.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Donor availability updated for user {UserId}: {IsAvailable}", userId, request.IsAvailable);
            return (true, $"Availability updated to {(request.IsAvailable ? "Available" : "Not Available")}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating donor availability for user {UserId}", userId);
            return (false, "An error occurred while updating availability");
        }
    }

    public async Task<(bool Success, string Message, List<DonorListItemDto>? Data)> GetAllDonorsAsync(int page, int pageSize, string? bloodGroup, bool? isAvailable)
    {
        try
        {
            var query = _context.Donors
                .Include(d => d.User)
                .Where(d => d.Status == DonorStatus.Active || d.Status == DonorStatus.Verified)
                .AsQueryable();

            // Filter by blood group
            if (!string.IsNullOrEmpty(bloodGroup) && Enum.TryParse<BloodGroup>(bloodGroup, out var bg))
            {
                query = query.Where(d => d.BloodGroup == bg);
            }

            // Filter by availability
            if (isAvailable.HasValue)
            {
                query = query.Where(d => d.IsAvailable == isAvailable.Value);
            }

            var donors = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var donorList = donors.Select(d => new DonorListItemDto
            {
                Id = d.Id,
                FullName = d.User.FullName,
                BloodGroup = d.BloodGroup,
                BloodGroupDisplay = GetBloodGroupDisplay(d.BloodGroup),
                Address = d.User.Address,
                IsAvailable = d.IsAvailable,
                Status = d.Status,
                StatusDisplay = d.Status.ToString(),
                TotalBloodDonations = d.TotalBloodDonations,
                CanDonateBlood = CanDonateBlood(d.LastBloodDonationDate),
                CreatedAt = d.CreatedAt
            }).ToList();

            return (true, "Donors retrieved successfully", donorList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving donors list");
            return (false, "An error occurred while retrieving donors", null);
        }
    }

    public async Task<(bool Success, string Message)> RecordBloodDonationAsync(int userId, DateTime donationDate)
    {
        try
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);

            if (donor == null)
            {
                return (false, "Donor profile not found");
            }

            donor.LastBloodDonationDate = donationDate;
            donor.TotalBloodDonations += 1;
            donor.UpdatedAt = TimeHelper.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Blood donation recorded for user {UserId}", userId);
            return (true, "Blood donation recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording blood donation for user {UserId}", userId);
            return (false, "An error occurred while recording donation");
        }
    }

    public async Task<bool> IsDonorAsync(int userId)
    {
        return await _context.Donors.AnyAsync(d => d.UserId == userId);
    }

    // Helper methods
    private Task<DonorProfileDto> GetDonorProfileDtoAsync(Donor donor, User user)
    {
        var (canDonate, daysUntil) = CalculateBloodDonationEligibility(donor.LastBloodDonationDate);
        
        var profile = new DonorProfileDto
        {
            Id = donor.Id,
            UserId = donor.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            DateOfBirth = user.DateOfBirth,
            BloodGroup = donor.BloodGroup,
            BloodGroupDisplay = GetBloodGroupDisplay(donor.BloodGroup),
            Weight = donor.Weight,
            Height = donor.Height,
            WillingToDonatePlasma = donor.WillingToDonatePlasma,
            WillingToDonatePlatelets = donor.WillingToDonatePlatelets,
            WillingToDonateOrgan = donor.WillingToDonateOrgan,
            WillingToDonateBoneMarrow = donor.WillingToDonateBoneMarrow,
            WillingToDonateEye = donor.WillingToDonateEye,
            PledgedOrgans = ParsePledgedOrgans(donor.PledgedOrgans),
            HasChronicDisease = donor.HasChronicDisease,
            ChronicDiseaseDetails = donor.ChronicDiseaseDetails,
            HasInfectiousDisease = donor.HasInfectiousDisease,
            IsSmoker = donor.IsSmoker,
            ConsumesAlcohol = donor.ConsumesAlcohol,
            Medications = donor.Medications,
            Allergies = donor.Allergies,
            LastBloodDonationDate = donor.LastBloodDonationDate,
            TotalBloodDonations = donor.TotalBloodDonations,
            CanDonateBlood = canDonate,
            DaysUntilCanDonate = daysUntil,
            IsAvailable = donor.IsAvailable,
            AvailabilityNotes = donor.AvailabilityNotes,
            EmergencyContactName = donor.EmergencyContactName,
            EmergencyContactPhone = donor.EmergencyContactPhone,
            EmergencyContactRelation = donor.EmergencyContactRelation,
            Status = donor.Status,
            StatusDisplay = donor.Status.ToString(),
            VerifiedAt = donor.VerifiedAt,
            DocumentPath = donor.DocumentPath,
            DocumentOriginalName = donor.DocumentOriginalName,
            DocumentUploadedAt = donor.DocumentUploadedAt,
            CreatedAt = donor.CreatedAt,
            UpdatedAt = donor.UpdatedAt
        };

        return Task.FromResult(profile);
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

    private static List<string>? ParsePledgedOrgans(string? pledgedOrgans)
    {
        if (string.IsNullOrEmpty(pledgedOrgans))
            return null;

        return pledgedOrgans.Split(',')
            .Select(o => int.TryParse(o, out var val) ? ((OrganType)val).ToString() : o)
            .ToList();
    }

    private static bool CanDonateBlood(DateTime? lastDonationDate)
    {
        if (!lastDonationDate.HasValue)
            return true;

        return (TimeHelper.Now - lastDonationDate.Value).TotalDays >= BloodDonationCooldownDays;
    }

    public async Task<(bool Success, string Message)> UpdateDocumentAsync(int userId, string documentPath, string originalName)
    {
        try
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (donor == null)
                return (false, "Donor profile not found");

            donor.DocumentPath = documentPath;
            donor.DocumentOriginalName = originalName;
            donor.DocumentUploadedAt = TimeHelper.Now;
            donor.UpdatedAt = TimeHelper.Now;

            await _context.SaveChangesAsync();
            return (true, "Document uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document for user {UserId}", userId);
            return (false, "Failed to update document");
        }
    }

    public async Task<(bool Success, string Message, string? OldPath)> DeleteDocumentAsync(int userId)
    {
        try
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (donor == null)
                return (false, "Donor profile not found", null);

            var oldPath = donor.DocumentPath;
            donor.DocumentPath = null;
            donor.DocumentOriginalName = null;
            donor.DocumentUploadedAt = null;
            donor.UpdatedAt = TimeHelper.Now;

            await _context.SaveChangesAsync();
            return (true, "Document deleted successfully", oldPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document for user {UserId}", userId);
            return (false, "Failed to delete document", null);
        }
    }

    private static (bool CanDonate, int DaysUntil) CalculateBloodDonationEligibility(DateTime? lastDonationDate)
    {
        if (!lastDonationDate.HasValue)
            return (true, 0);

        var daysSinceDonation = (TimeHelper.Now - lastDonationDate.Value).TotalDays;
        
        if (daysSinceDonation >= BloodDonationCooldownDays)
            return (true, 0);

        return (false, (int)(BloodDonationCooldownDays - daysSinceDonation));
    }

    /// <summary>
    /// Get all active donation requests that match the donor's blood type
    /// </summary>
    public async Task<(bool Success, string Message, List<IncomingDonationRequestDto>? Data)> GetIncomingRequestsAsync(int userId)
    {
        try
        {
            // Get donor profile
            var donor = await _context.Donors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (donor == null)
            {
                return (false, "Donor profile not found", null);
            }

            // Only Active donors can view incoming requests
            if (donor.Status != DonorStatus.Active)
            {
                return (false, "Your profile has not been verified by a hospital yet. You cannot receive donation requests until your profile is approved.", new List<IncomingDonationRequestDto>());
            }

            // Log donor's blood group for debugging
            _logger.LogInformation("Donor {UserId} has blood group {BloodGroup} (value: {BloodGroupValue})", 
                userId, donor.BloodGroup.ToString(), (int)donor.BloodGroup);

            // Get compatible blood groups for this donor (recipients who can receive from this donor)
            var compatibleBloodGroups = GetCompatibleRecipientBloodGroups(donor.BloodGroup);
            
            _logger.LogInformation("Compatible recipient blood groups for donor: {Groups}", 
                string.Join(", ", compatibleBloodGroups.Select(bg => $"{bg}({(int)bg})")));

            // Get all active donation requests first (client-side evaluation needed for Contains)
            var allActiveRequests = await _context.DonationRequests
                .Include(r => r.Recipient)
                    .ThenInclude(rec => rec.User)
                .Where(r => 
                    r.Status == RequestStatus.Pending || r.Status == RequestStatus.Sent)
                .ToListAsync();

            _logger.LogInformation("Found {Count} active donation requests before filtering", allActiveRequests.Count);
            
            // Log all request blood groups for debugging
            foreach (var req in allActiveRequests)
            {
                _logger.LogInformation("Request {Id}: BloodGroup={BloodGroup} (value: {Value}), IsMatch={IsMatch}", 
                    req.Id, req.BloodGroup.ToString(), (int)req.BloodGroup, 
                    compatibleBloodGroups.Contains(req.BloodGroup));
            }

            // Filter by compatible blood groups in memory (MySQL/Pomelo can't translate Contains on primitive collections)
            // Create a set of compatible blood group integer values for faster lookup
            var compatibleBloodGroupValues = new HashSet<int>(compatibleBloodGroups.Select(bg => (int)bg));

            var requests = allActiveRequests
                .Where(r => compatibleBloodGroupValues.Contains((int)r.BloodGroup))
                .OrderByDescending(r => r.UrgencyLevel)
                .ThenBy(r => r.RequiredDateTime)
                .ToList();

            _logger.LogInformation("After blood group filtering: {Count} requests match", requests.Count);

            // Check if donor has already responded to any of these
            var donorResponses = await _context.DonorRequests
                .Where(dr => dr.DonorId == donor.Id)
                .ToDictionaryAsync(dr => dr.DonationRequestId, dr => dr);

            var result = requests.Select(r => new IncomingDonationRequestDto
            {
                RequestId = r.Id,
                RecipientName = r.Recipient?.User?.FullName ?? "Anonymous",
                BloodGroup = r.BloodGroup.ToString(),
                DonationType = r.DonationType.ToString(),
                Quantity = r.Quantity,
                UrgencyLevel = r.UrgencyLevel.ToString(),
                RequiredDateTime = r.RequiredDateTime,
                HospitalName = r.HospitalName,
                HospitalLocation = r.HospitalLocation,
                City = r.City,
                ContactName = r.ContactName,
                ContactPhone = r.ContactPhone,
                MedicalNotes = r.MedicalNotes,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt,
                DonorResponseStatus = donorResponses.TryGetValue(r.Id, out var response) 
                    ? response.Status.ToString() 
                    : null,
                DonorRespondedAt = donorResponses.TryGetValue(r.Id, out var resp) 
                    ? resp.RespondedAt 
                    : null,
                DonorRequestId = donorResponses.TryGetValue(r.Id, out var dr) 
                    ? dr.Id 
                    : null
            }).ToList();

            return (true, $"Found {result.Count} matching donation requests", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incoming requests for user {UserId}", userId);
            return (false, "An error occurred while retrieving requests", null);
        }
    }

    /// <summary>
    /// Respond to a donation request (accept or decline)
    /// </summary>
    public async Task<(bool Success, string Message)> RespondToRequestAsync(int userId, int requestId, bool accept, string? notes)
    {
        try
        {
            // Get donor profile
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (donor == null)
            {
                return (false, "Donor profile not found");
            }

            // Only Active donors can respond to requests
            if (donor.Status != DonorStatus.Active)
            {
                return (false, "Your profile has not been verified by a hospital. You cannot respond to donation requests until approved.");
            }

            // Get the donation request
            var request = await _context.DonationRequests.FindAsync(requestId);
            if (request == null)
            {
                return (false, "Donation request not found");
            }

            // Check if already responded
            var existingResponse = await _context.DonorRequests
                .FirstOrDefaultAsync(dr => dr.DonorId == donor.Id && dr.DonationRequestId == requestId);

            if (existingResponse != null)
            {
                // Update existing response
                existingResponse.Status = accept ? RequestStatus.Accepted : RequestStatus.Cancelled;
                existingResponse.RespondedAt = TimeHelper.Now;
                existingResponse.ResponseNotes = notes;
            }
            else
            {
                // Create new response
                var donorRequest = new DonorRequest
                {
                    DonationRequestId = requestId,
                    DonorId = donor.Id,
                    Status = accept ? RequestStatus.Accepted : RequestStatus.Cancelled,
                    IsNotified = true,
                    NotifiedAt = TimeHelper.Now,
                    RespondedAt = TimeHelper.Now,
                    ResponseNotes = notes,
                    CreatedAt = TimeHelper.Now
                };
                _context.DonorRequests.Add(donorRequest);
            }

            // Update request stats and status
            if (accept)
            {
                request.AcceptedDonorsCount++;
                // Update the donation request status to Accepted if not already completed
                if (request.Status != RequestStatus.Completed)
                {
                    request.Status = RequestStatus.Accepted;
                }
            }
            request.UpdatedAt = TimeHelper.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Donor {DonorId} {Action} request {RequestId}", 
                donor.Id, accept ? "accepted" : "declined", requestId);

            return (true, accept ? "You have accepted this donation request" : "You have declined this request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to request {RequestId} by user {UserId}", requestId, userId);
            return (false, "An error occurred while processing your response");
        }
    }

    /// <summary>
    /// Get the donor's own donation history (all requests they responded to)
    /// </summary>
    public async Task<(bool Success, string Message, List<DonorDonationHistoryDto>? Data)> GetDonationHistoryAsync(int userId)
    {
        try
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (donor == null)
            {
                return (false, "Donor profile not found", null);
            }

            var donorRequests = await _context.DonorRequests
                .Include(dr => dr.DonationRequest)
                    .ThenInclude(r => r.Recipient)
                        .ThenInclude(rec => rec.User)
                .Where(dr => dr.DonorId == donor.Id)
                .OrderByDescending(dr => dr.RespondedAt ?? dr.CreatedAt)
                .ToListAsync();

            var history = donorRequests.Select(dr => new DonorDonationHistoryDto
            {
                DonorRequestId = dr.Id,
                DonationRequestId = dr.DonationRequestId,
                RecipientName = dr.DonationRequest.Recipient?.User?.FullName ?? "Anonymous",
                BloodGroup = dr.DonationRequest.BloodGroup,
                BloodGroupDisplay = GetBloodGroupDisplay(dr.DonationRequest.BloodGroup),
                DonationType = dr.DonationRequest.DonationType,
                DonationTypeDisplay = dr.DonationRequest.DonationType.ToString(),
                Quantity = dr.DonationRequest.Quantity,
                UrgencyLevel = dr.DonationRequest.UrgencyLevel,
                UrgencyLevelDisplay = dr.DonationRequest.UrgencyLevel.ToString(),
                HospitalName = dr.DonationRequest.HospitalName,
                HospitalLocation = dr.DonationRequest.HospitalLocation,
                City = dr.DonationRequest.City,
                Status = dr.Status,
                StatusDisplay = dr.Status.ToString(),
                RespondedAt = dr.RespondedAt,
                RequestCreatedAt = dr.DonationRequest.CreatedAt,
                RequiredDateTime = dr.DonationRequest.RequiredDateTime,
                RequestCompletedAt = dr.DonationRequest.CompletedAt,
                ResponseNotes = dr.ResponseNotes
            }).ToList();

            return (true, $"Found {history.Count} donation records", history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting donation history for user {UserId}", userId);
            return (false, "An error occurred while retrieving donation history", null);
        }
    }

    /// <summary>
    /// Get blood groups that this donor can donate to
    /// Based on blood type compatibility
    /// </summary>
    private static List<BloodGroup> GetCompatibleRecipientBloodGroups(BloodGroup donorBloodGroup)
    {
        return donorBloodGroup switch
        {
            // O- can donate to everyone
            BloodGroup.ONegative => new List<BloodGroup> 
            { 
                BloodGroup.APositive, BloodGroup.ANegative,
                BloodGroup.BPositive, BloodGroup.BNegative,
                BloodGroup.ABPositive, BloodGroup.ABNegative,
                BloodGroup.OPositive, BloodGroup.ONegative 
            },
            // O+ can donate to A+, B+, AB+, O+
            BloodGroup.OPositive => new List<BloodGroup> 
            { 
                BloodGroup.APositive, BloodGroup.BPositive,
                BloodGroup.ABPositive, BloodGroup.OPositive 
            },
            // A- can donate to A+, A-, AB+, AB-
            BloodGroup.ANegative => new List<BloodGroup> 
            { 
                BloodGroup.APositive, BloodGroup.ANegative,
                BloodGroup.ABPositive, BloodGroup.ABNegative 
            },
            // A+ can donate to A+, AB+
            BloodGroup.APositive => new List<BloodGroup> 
            { 
                BloodGroup.APositive, BloodGroup.ABPositive 
            },
            // B- can donate to B+, B-, AB+, AB-
            BloodGroup.BNegative => new List<BloodGroup> 
            { 
                BloodGroup.BPositive, BloodGroup.BNegative,
                BloodGroup.ABPositive, BloodGroup.ABNegative 
            },
            // B+ can donate to B+, AB+
            BloodGroup.BPositive => new List<BloodGroup> 
            { 
                BloodGroup.BPositive, BloodGroup.ABPositive 
            },
            // AB- can donate to AB+, AB-
            BloodGroup.ABNegative => new List<BloodGroup> 
            { 
                BloodGroup.ABPositive, BloodGroup.ABNegative 
            },
            // AB+ can only donate to AB+
            BloodGroup.ABPositive => new List<BloodGroup> 
            { 
                BloodGroup.ABPositive 
            },
            _ => new List<BloodGroup>()
        };
    }

    public async Task<(bool Success, string Message, DonorOfferDto? Data)> CreateDonorOfferAsync(int userId, CreateDonorOfferDto request)
    {
        try
        {
            var donor = await _context.Donors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (donor == null)
                return (false, "Donor profile not found", null);

            if (donor.Status != DonorStatus.Active)
                return (false, "Only verified (Active) donors can create donation offers", null);

            var offer = new DonorOffer
            {
                DonorId = donor.Id,
                DonationType = request.DonationType,
                Quantity = request.Quantity,
                HospitalName = request.HospitalName,
                HospitalLocation = request.HospitalLocation,
                City = request.City,
                PreferredDate = request.PreferredDate,
                Notes = request.Notes,
                Status = DonorOfferStatus.Available,
                CreatedAt = TimeHelper.Now
            };

            _context.DonorOffers.Add(offer);
            await _context.SaveChangesAsync();

            var dto = MapToOfferDto(offer, donor);
            return (true, "Donation offer created successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating donor offer for user {UserId}", userId);
            return (false, "An error occurred while creating the donation offer", null);
        }
    }

    public async Task<(bool Success, string Message, List<DonorOfferDto>? Data)> GetDonorOffersAsync(int userId)
    {
        try
        {
            var donor = await _context.Donors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (donor == null)
                return (false, "Donor profile not found", null);

            var offers = await _context.DonorOffers
                .Where(o => o.DonorId == donor.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var dtos = offers.Select(o => MapToOfferDto(o, donor)).ToList();
            return (true, "Donor offers retrieved", dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting donor offers for user {UserId}", userId);
            return (false, "An error occurred while retrieving offers", null);
        }
    }

    public async Task<(bool Success, string Message)> CancelDonorOfferAsync(int userId, int offerId)
    {
        try
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (donor == null)
                return (false, "Donor profile not found");

            var offer = await _context.DonorOffers
                .FirstOrDefaultAsync(o => o.Id == offerId && o.DonorId == donor.Id);

            if (offer == null)
                return (false, "Donation offer not found");

            if (offer.Status == DonorOfferStatus.Cancelled)
                return (false, "Offer is already cancelled");

            offer.Status = DonorOfferStatus.Cancelled;
            offer.UpdatedAt = TimeHelper.Now;
            await _context.SaveChangesAsync();

            return (true, "Donation offer cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling donor offer {OfferId} for user {UserId}", offerId, userId);
            return (false, "An error occurred while cancelling the offer");
        }
    }

    private static DonorOfferDto MapToOfferDto(DonorOffer offer, Donor donor)
    {
        return new DonorOfferDto
        {
            Id = offer.Id,
            DonorName = donor.User.FullName,
            BloodGroup = donor.BloodGroup,
            BloodGroupDisplay = donor.BloodGroup.ToString(),
            DonationType = offer.DonationType,
            DonationTypeDisplay = offer.DonationType.ToString(),
            Quantity = offer.Quantity,
            HospitalName = offer.HospitalName,
            HospitalLocation = offer.HospitalLocation,
            City = offer.City,
            PreferredDate = offer.PreferredDate,
            Notes = offer.Notes,
            Status = offer.Status,
            StatusDisplay = offer.Status.ToString(),
            CreatedAt = offer.CreatedAt
        };
    }
}
