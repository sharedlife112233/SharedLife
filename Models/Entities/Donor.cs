using SharedLife.Models.Enums;
using SharedLife.Utilities;

namespace SharedLife.Models.Entities;

public class Donor
{
    public int Id { get; set; }
    
    // Foreign key to User
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    // Medical Information
    public BloodGroup BloodGroup { get; set; }
    public double? Weight { get; set; } // in kg
    public double? Height { get; set; } // in cm
    
    // Donation Preferences
    public bool WillingToDonatePlasma { get; set; } = false;
    public bool WillingToDonatePlatelets { get; set; } = false;
    public bool WillingToDonateOrgan { get; set; } = false;
    public bool WillingToDonateBoneMarrow { get; set; } = false;
    public bool WillingToDonateEye { get; set; } = false;
    
    // Organ Pledge (comma-separated organ types if willing to donate)
    public string? PledgedOrgans { get; set; }
    
    // Health Information
    public bool HasChronicDisease { get; set; } = false;
    public string? ChronicDiseaseDetails { get; set; }
    public bool HasInfectiousDisease { get; set; } = false;
    public bool IsSmoker { get; set; } = false;
    public bool ConsumesAlcohol { get; set; } = false;
    public string? Medications { get; set; }
    public string? Allergies { get; set; }
    
    // Last Donation Info
    public DateTime? LastBloodDonationDate { get; set; }
    public int TotalBloodDonations { get; set; } = 0;
    
    // Availability
    public bool IsAvailable { get; set; } = true;
    public string? AvailabilityNotes { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    
    // Verification
    public DonorStatus Status { get; set; } = DonorStatus.Pending;
    public int? VerifiedByHospitalId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;
    public DateTime? UpdatedAt { get; set; }
}
