using SharedLife.Models.Enums;

namespace SharedLife.Models.Entities;

/// <summary>
/// Junction table between DonationRequest and Donor
/// Tracks individual donor responses to donation requests
/// </summary>
public class DonorRequest
{
    public int Id { get; set; }
    
    // Foreign keys
    public int DonationRequestId { get; set; }
    public virtual DonationRequest DonationRequest { get; set; } = null!;
    
    public int DonorId { get; set; }
    public virtual Donor Donor { get; set; } = null!;
    
    // Response tracking
    public RequestStatus Status { get; set; } = RequestStatus.Sent;
    public bool IsNotified { get; set; } = false;
    public DateTime? NotifiedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ResponseNotes { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
