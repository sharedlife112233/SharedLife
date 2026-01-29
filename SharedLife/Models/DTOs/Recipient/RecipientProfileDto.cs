using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Recipient;

public class RecipientProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    // User Info
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    
    // Medical Info
    public BloodGroup BloodGroup { get; set; }
    public string BloodGroupDisplay { get; set; } = string.Empty;
    public string? MedicalCondition { get; set; }
    
    // Hospital Info
    public string? HospitalName { get; set; }
    public string? HospitalAddress { get; set; }
    public string? City { get; set; }
    
    // Doctor Info
    public string? DoctorName { get; set; }
    public string? DoctorContact { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    
    // Status
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Stats
    public int TotalRequests { get; set; }
    public int ActiveRequests { get; set; }
    public int CompletedRequests { get; set; }
}
