using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Donor;

/// <summary>
/// DTO for displaying a donor's own donation history
/// </summary>
public class DonorDonationHistoryDto
{
    public int DonorRequestId { get; set; }
    public int DonationRequestId { get; set; }

    // Recipient Info
    public string RecipientName { get; set; } = string.Empty;

    // Donation Details
    public BloodGroup BloodGroup { get; set; }
    public string BloodGroupDisplay { get; set; } = string.Empty;
    public DonationType DonationType { get; set; }
    public string DonationTypeDisplay { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public UrgencyLevel UrgencyLevel { get; set; }
    public string UrgencyLevelDisplay { get; set; } = string.Empty;

    // Hospital Info
    public string HospitalName { get; set; } = string.Empty;
    public string HospitalLocation { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    // Status & Dates
    public RequestStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime? RespondedAt { get; set; }
    public DateTime RequestCreatedAt { get; set; }
    public DateTime RequiredDateTime { get; set; }
    public DateTime? RequestCompletedAt { get; set; }
    public string? ResponseNotes { get; set; }
}
