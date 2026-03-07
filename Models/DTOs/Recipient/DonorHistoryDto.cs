using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Recipient;

public class DonorHistoryDto
{
    public int DonorRequestId { get; set; }
    public int DonationRequestId { get; set; }
    
    // Donor Info
    public int DonorId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public BloodGroup DonorBloodGroup { get; set; }
    public string DonorBloodGroupDisplay { get; set; } = string.Empty;
    public string? DonorCity { get; set; }
    public string? DonorPhone { get; set; }
    
    // Donation Request Info
    public DonationType DonationType { get; set; }
    public string DonationTypeDisplay { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public UrgencyLevel UrgencyLevel { get; set; }
    public string UrgencyLevelDisplay { get; set; } = string.Empty;
    public string HospitalName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Status & Dates
    public RequestStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime? RespondedAt { get; set; }
    public DateTime RequestCreatedAt { get; set; }
    public DateTime? RequestCompletedAt { get; set; }
    public string? ResponseNotes { get; set; }
}
