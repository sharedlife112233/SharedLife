namespace SharedLife.Models.DTOs.Admin;

public class HospitalListDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string HospitalType { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool HasBloodBank { get; set; }
    public bool HasOrganTransplant { get; set; }
    public bool HasEyeBank { get; set; }
    public bool HasBoneMarrowRegistry { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int? VerifiedByAdminId { get; set; }
    public string? VerificationNotes { get; set; }
    public int TotalDonorsVerified { get; set; }
    public int TotalRequestsProcessed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class HospitalListResponseDto
{
    public List<HospitalListDto> Hospitals { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class VerifyHospitalDto
{
    public string? Notes { get; set; }
}
