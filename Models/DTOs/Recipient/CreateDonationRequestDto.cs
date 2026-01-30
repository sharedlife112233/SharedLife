using System.ComponentModel.DataAnnotations;
using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Recipient;

public class CreateDonationRequestDto
{
    [Required(ErrorMessage = "Blood group is required")]
    public BloodGroup BloodGroup { get; set; }
    
    [Required(ErrorMessage = "Donation type is required")]
    public DonationType DonationType { get; set; }
    
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10 units")]
    public int Quantity { get; set; }
    
    [Required(ErrorMessage = "Urgency level is required")]
    public UrgencyLevel UrgencyLevel { get; set; }
    
    [Required(ErrorMessage = "Required date/time is required")]
    public DateTime RequiredDateTime { get; set; }
    
    [Required(ErrorMessage = "Hospital name is required")]
    [StringLength(200, ErrorMessage = "Hospital name cannot exceed 200 characters")]
    public string HospitalName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Hospital location is required")]
    [StringLength(500, ErrorMessage = "Hospital location cannot exceed 500 characters")]
    public string HospitalLocation { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "City is required")]
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Contact name is required")]
    [StringLength(100, ErrorMessage = "Contact name cannot exceed 100 characters")]
    public string ContactName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Contact phone is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string ContactPhone { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Doctor name cannot exceed 100 characters")]
    public string? DoctorName { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? DoctorContact { get; set; }
    
    [StringLength(1000, ErrorMessage = "Medical notes cannot exceed 1000 characters")]
    public string? MedicalNotes { get; set; }
    
    [StringLength(500, ErrorMessage = "Additional requirements cannot exceed 500 characters")]
    public string? AdditionalRequirements { get; set; }
}
