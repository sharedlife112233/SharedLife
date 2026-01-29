using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Recipient;

public class DonationRequestDto
{
    public int Id { get; set; }
    public int RecipientId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    
    // Request Details
    public BloodGroup BloodGroup { get; set; }
    public string BloodGroupDisplay { get; set; } = string.Empty;
    public DonationType DonationType { get; set; }
    public string DonationTypeDisplay { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public UrgencyLevel UrgencyLevel { get; set; }
    public string UrgencyLevelDisplay { get; set; } = string.Empty;
    public DateTime RequiredDateTime { get; set; }
    
    // Hospital Info
    public string HospitalName { get; set; } = string.Empty;
    public string HospitalLocation { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Contact Info
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public string? DoctorContact { get; set; }
    
    // Additional Info
    public string? MedicalNotes { get; set; }
    public string? AdditionalRequirements { get; set; }
    
    // Status
    public RequestStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public int MatchedDonorsCount { get; set; }
    public int AcceptedDonorsCount { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
