using SharedLife.Models.Enums;

namespace SharedLife.Models.Entities;

public class Recipient
{
    public int Id { get; set; }
    
    // Foreign key to User
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    // Medical Information
    public BloodGroup BloodGroup { get; set; }
    public string? MedicalCondition { get; set; }
    public string? HospitalName { get; set; }
    public string? HospitalAddress { get; set; }
    public string? City { get; set; }
    
    // Doctor Information
    public string? DoctorName { get; set; }
    public string? DoctorContact { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    
    // Verification
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();
}
