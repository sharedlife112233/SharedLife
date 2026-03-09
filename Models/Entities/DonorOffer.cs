using SharedLife.Models.Enums;
using SharedLife.Utilities;

namespace SharedLife.Models.Entities;

public class DonorOffer
{
    public int Id { get; set; }
    
    // Foreign key to Donor
    public int DonorId { get; set; }
    public virtual Donor Donor { get; set; } = null!;
    
    // Donation Details
    public DonationType DonationType { get; set; }
    public int Quantity { get; set; } = 1;
    
    // Hospital / Location
    public string HospitalName { get; set; } = string.Empty;
    public string HospitalLocation { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Schedule
    public DateTime PreferredDate { get; set; }
    
    // Additional Info
    public string? Notes { get; set; }
    
    // Status Tracking
    public DonorOfferStatus Status { get; set; } = DonorOfferStatus.Available;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;
    public DateTime? UpdatedAt { get; set; }
}
