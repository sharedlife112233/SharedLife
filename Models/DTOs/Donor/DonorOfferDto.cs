using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Donor;

public class DonorOfferDto
{
    public int Id { get; set; }
    
    // Donor Info (auto-filled from profile)
    public string DonorName { get; set; } = string.Empty;
    public BloodGroup BloodGroup { get; set; }
    public string BloodGroupDisplay { get; set; } = string.Empty;
    
    // Donation Details
    public DonationType DonationType { get; set; }
    public string DonationTypeDisplay { get; set; } = string.Empty;
    public int Quantity { get; set; }
    
    // Hospital / Location
    public string HospitalName { get; set; } = string.Empty;
    public string HospitalLocation { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Schedule
    public DateTime PreferredDate { get; set; }
    
    // Additional Info
    public string? Notes { get; set; }
    
    // Status
    public DonorOfferStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
}
