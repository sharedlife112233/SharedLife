using SharedLife.Models.Enums;
using SharedLife.Utilities;

namespace SharedLife.Models.Entities;

public class DonationRequest
{
    public int Id { get; set; }
    
    // Foreign key to Recipient
    public int RecipientId { get; set; }
    public virtual Recipient Recipient { get; set; } = null!;
    
    // Request Details
    public BloodGroup BloodGroup { get; set; }
    public DonationType DonationType { get; set; }
    public int Quantity { get; set; } // Units needed
    public UrgencyLevel UrgencyLevel { get; set; }
    public DateTime RequiredDateTime { get; set; }
    
    // Hospital Information
    public string HospitalName { get; set; } = string.Empty;
    public string HospitalLocation { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Contact Details
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    
    // Additional Information
    public string? MedicalNotes { get; set; }
    public string? AdditionalRequirements { get; set; }
    
    // Status Tracking
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public int MatchedDonorsCount { get; set; } = 0;
    public int AcceptedDonorsCount { get; set; } = 0;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<DonorRequest> DonorRequests { get; set; } = new List<DonorRequest>();
}
