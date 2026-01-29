namespace SharedLife.Models.DTOs.Admin;

public class RecipientListDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string BloodGroupDisplay { get; set; } = string.Empty;
    public string? MedicalCondition { get; set; }
    public string? HospitalName { get; set; }
    public string? City { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorContact { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int CompletedRequests { get; set; }
}

public class RecipientListResponseDto
{
    public List<RecipientListDto> Recipients { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
