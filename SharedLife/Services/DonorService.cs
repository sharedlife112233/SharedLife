using Microsoft.EntityFrameworkCore;
using SharedLife.Data;
using SharedLife.Models.DTOs.Donor;
using SharedLife.Models.Entities;
using SharedLife.Models.Enums;
using SharedLife.Services.Interfaces;

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
                CreatedAt = DateTime.UtcNow
            };

            // Update user's blood group if not set
            if (user.BloodGroup == null)
            {
                user.BloodGroup = request.BloodGroup;
                user.UpdatedAt = DateTime.UtcNow;
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
            donor.UpdatedAt = DateTime.UtcNow;

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
            donor.UpdatedAt = DateTime.UtcNow;

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
            donor.UpdatedAt = DateTime.UtcNow;

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

        return (DateTime.UtcNow - lastDonationDate.Value).TotalDays >= BloodDonationCooldownDays;
    }

    private static (bool CanDonate, int DaysUntil) CalculateBloodDonationEligibility(DateTime? lastDonationDate)
    {
        if (!lastDonationDate.HasValue)
            return (true, 0);

        var daysSinceDonation = (DateTime.UtcNow - lastDonationDate.Value).TotalDays;
        
        if (daysSinceDonation >= BloodDonationCooldownDays)
            return (true, 0);

        return (false, (int)(BloodDonationCooldownDays - daysSinceDonation));
    }
}
