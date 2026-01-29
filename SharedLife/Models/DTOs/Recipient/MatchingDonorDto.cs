using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Recipient;

public class MatchingDonorDto
{
    public int DonorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public BloodGroup BloodGroup { get; set; }
    public string BloodGroupDisplay { get; set; } = string.Empty;
    public string? City { get; set; }
    public bool IsAvailable { get; set; }
    public DonorStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    
    // Donation preferences (for privacy, only show relevant info)
    public bool WillingToDonatePlasma { get; set; }
    public bool WillingToDonatePlatelets { get; set; }
    public bool WillingToDonateOrgan { get; set; }
    public bool WillingToDonateBoneMarrow { get; set; }
    public bool WillingToDonateEye { get; set; }
    
    // Eligibility
    public bool CanDonateBlood { get; set; }
    public int? DaysUntilCanDonate { get; set; }
    public int TotalBloodDonations { get; set; }
}
