using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Hospital;

public class PendingDonorDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    // User Info
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    
    // Donor Info
    public string BloodGroup { get; set; } = string.Empty;
    public double? Weight { get; set; }
    public double? Height { get; set; }
    
    // Donation Preferences
    public bool WillingToDonateBlood { get; set; }
    public bool WillingToDonatePlasma { get; set; }
    public bool WillingToDonatePlatelets { get; set; }
    public bool WillingToDonateOrgan { get; set; }
    public bool WillingToDonateBoneMarrow { get; set; }
    public bool WillingToDonateEye { get; set; }
    public string? PledgedOrgans { get; set; }
    
    // Health Info
    public bool HasChronicDisease { get; set; }
    public string? ChronicDiseaseDetails { get; set; }
    public bool HasInfectiousDisease { get; set; }
    public bool IsSmoker { get; set; }
    public bool ConsumesAlcohol { get; set; }
    public string? Medications { get; set; }
    public string? Allergies { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    
    // Status
    public DonorStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Document
    public string? DocumentPath { get; set; }
    public string? DocumentOriginalName { get; set; }
    public DateTime? DocumentUploadedAt { get; set; }
    public bool HasDocument => !string.IsNullOrEmpty(DocumentPath);
}
