using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Donor;

public class DonorProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    // User Info (from User entity)
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    
    // Medical Information
    public BloodGroup BloodGroup { get; set; }
    public string BloodGroupDisplay { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public double? Height { get; set; }
    
    // Donation Preferences
    public bool WillingToDonateBlood { get; set; }
    public bool WillingToDonatePlasma { get; set; }
    public bool WillingToDonatePlatelets { get; set; }
    public bool WillingToDonateOrgan { get; set; }
    public bool WillingToDonateBoneMarrow { get; set; }
    public bool WillingToDonateEye { get; set; }
    public List<string>? PledgedOrgans { get; set; }
    
    // Health Information
    public bool HasChronicDisease { get; set; }
    public string? ChronicDiseaseDetails { get; set; }
    public bool HasInfectiousDisease { get; set; }
    public bool IsSmoker { get; set; }
    public bool ConsumesAlcohol { get; set; }
    public string? Medications { get; set; }
    public string? Allergies { get; set; }
    
    // Donation History
    public DateTime? LastBloodDonationDate { get; set; }
    public int TotalBloodDonations { get; set; }
    public bool CanDonateBlood { get; set; } // Calculated based on last donation
    public int DaysUntilCanDonate { get; set; } // Days until eligible
    
    // Availability
    public bool IsAvailable { get; set; }
    public string? AvailabilityNotes { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    
    // Status
    public DonorStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime? VerifiedAt { get; set; }
    
    // Document
    public string? DocumentPath { get; set; }
    public string? DocumentOriginalName { get; set; }
    public DateTime? DocumentUploadedAt { get; set; }
    public bool HasDocument => !string.IsNullOrEmpty(DocumentPath);
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
