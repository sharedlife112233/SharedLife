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

            // Update request stats
            if (accept)
            {
                request.AcceptedDonorsCount++;
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
}
