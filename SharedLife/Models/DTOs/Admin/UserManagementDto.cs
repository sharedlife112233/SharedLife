namespace SharedLife.Models.DTOs.Admin;

public class UserManagementDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? BloodGroup { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool HasDonorProfile { get; set; }
    public bool HasRecipientProfile { get; set; }
}

public class UserListResponseDto
{
    public List<UserManagementDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UserDetailsDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? BloodGroup { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Donor info if exists
    public DonorInfoDto? DonorInfo { get; set; }
    
    // Recipient info if exists
    public RecipientInfoDto? RecipientInfo { get; set; }
}

public class DonorInfoDto
{
    public int Id { get; set; }
    public string BloodGroup { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int TotalBloodDonations { get; set; }
    public DateTime? LastBloodDonationDate { get; set; }
    public bool WillingToDonatePlasma { get; set; }
    public bool WillingToDonatePlatelets { get; set; }
    public bool WillingToDonateOrgan { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RecipientInfoDto
{
    public int Id { get; set; }
    public string BloodGroup { get; set; } = string.Empty;
    public string? MedicalCondition { get; set; }
    public string? HospitalName { get; set; }
    public string? City { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
}

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }
}
