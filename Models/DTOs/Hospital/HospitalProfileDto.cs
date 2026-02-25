namespace SharedLife.Models.DTOs.Hospital;

public class HospitalProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    // User Information
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    
    // Hospital Information
    public string HospitalName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string HospitalType { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    
    // Contact Information
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public string? Website { get; set; }
    
    // Services
    public bool HasBloodBank { get; set; }
    public bool HasOrganTransplant { get; set; }
    public bool HasEyeBank { get; set; }
    public bool HasBoneMarrowRegistry { get; set; }
    
    // Capacity
    public int? BloodBankCapacity { get; set; }
    public int? BedCapacity { get; set; }
    
    // Operating Hours
    public string? OperatingHours { get; set; }
    public bool IsOpen24x7 { get; set; }
    
    // Verification
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }
    
    // Status
    public bool IsActive { get; set; }
    
    // Statistics
    public int TotalDonorsVerified { get; set; }
    public int TotalRequestsProcessed { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
