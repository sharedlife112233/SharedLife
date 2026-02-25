using SharedLife.Models.Enums;
using SharedLife.Utilities;

namespace SharedLife.Models.Entities;

public class Hospital
{
    public int Id { get; set; }
    
    // Foreign key to User
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    // Hospital Information
    public string HospitalName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string HospitalType { get; set; } = string.Empty; // Government, Private, NGO
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
    public bool HasBloodBank { get; set; } = false;
    public bool HasOrganTransplant { get; set; } = false;
    public bool HasEyeBank { get; set; } = false;
    public bool HasBoneMarrowRegistry { get; set; } = false;
    
    // Capacity
    public int? BloodBankCapacity { get; set; }
    public int? BedCapacity { get; set; }
    
    // Operating Hours
    public string? OperatingHours { get; set; }
    public bool IsOpen24x7 { get; set; } = false;
    
    // Verification
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public int? VerifiedByAdminId { get; set; }
    public string? VerificationNotes { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;
    public DateTime? UpdatedAt { get; set; }
    
    // Statistics (computed/cached)
    public int TotalDonorsVerified { get; set; } = 0;
    public int TotalRequestsProcessed { get; set; } = 0;
}
