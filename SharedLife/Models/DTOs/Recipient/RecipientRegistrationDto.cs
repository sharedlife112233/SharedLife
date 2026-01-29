using System.ComponentModel.DataAnnotations;
using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Recipient;

public class RecipientRegistrationDto
{
    [Required(ErrorMessage = "Blood group is required")]
    public BloodGroup BloodGroup { get; set; }
    
    public string? MedicalCondition { get; set; }
    
    [StringLength(200, ErrorMessage = "Hospital name cannot exceed 200 characters")]
    public string? HospitalName { get; set; }
    
    [StringLength(500, ErrorMessage = "Hospital address cannot exceed 500 characters")]
    public string? HospitalAddress { get; set; }
    
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }
    
    [StringLength(100, ErrorMessage = "Doctor name cannot exceed 100 characters")]
    public string? DoctorName { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? DoctorContact { get; set; }
    
    [Required(ErrorMessage = "Emergency contact name is required")]
    [StringLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
    public string EmergencyContactName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Emergency contact phone is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string EmergencyContactPhone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Emergency contact relation is required")]
    [StringLength(50, ErrorMessage = "Emergency contact relation cannot exceed 50 characters")]
    public string EmergencyContactRelation { get; set; } = string.Empty;
}
