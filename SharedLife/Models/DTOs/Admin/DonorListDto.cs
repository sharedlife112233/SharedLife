namespace SharedLife.Models.DTOs.Admin;

public class DonorListDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string BloodGroupDisplay { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool IsVerified { get; set; }
    public int TotalBloodDonations { get; set; }
    public DateTime? LastBloodDonationDate { get; set; }
    public bool WillingToDonatePlasma { get; set; }
    public bool WillingToDonatePlatelets { get; set; }
    public bool WillingToDonateOrgan { get; set; }
    public bool WillingToDonateBoneMarrow { get; set; }
    public bool WillingToDonateEye { get; set; }
    public double? Weight { get; set; }
    public double? Height { get; set; }
    public bool HasChronicDisease { get; set; }
    public bool HasInfectiousDisease { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class DonorListResponseDto
{
    public List<DonorListDto> Donors { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
