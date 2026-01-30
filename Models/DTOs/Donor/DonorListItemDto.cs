using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Donor;

public class DonorListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public BloodGroup BloodGroup { get; set; }
    public string BloodGroupDisplay { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsAvailable { get; set; }
    public DonorStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public int TotalBloodDonations { get; set; }
    public bool CanDonateBlood { get; set; }
    public DateTime CreatedAt { get; set; }
}
