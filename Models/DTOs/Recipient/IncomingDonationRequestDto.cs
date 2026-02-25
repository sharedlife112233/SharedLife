using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Recipient;

/// <summary>
/// DTO for displaying donation requests to donors
/// </summary>
public class IncomingDonationRequestDto
{
    public int RequestId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string DonationType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string UrgencyLevel { get; set; } = string.Empty;
    public DateTime RequiredDateTime { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string HospitalLocation { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? MedicalNotes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Donor's response status to this request (if any)
    public string? DonorResponseStatus { get; set; }
    public DateTime? DonorRespondedAt { get; set; }
    
    // DonorRequest ID for chat functionality (only available when donor has responded)
    public int? DonorRequestId { get; set; }
}
