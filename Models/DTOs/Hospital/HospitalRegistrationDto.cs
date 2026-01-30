using System.ComponentModel.DataAnnotations;

namespace SharedLife.Models.DTOs.Hospital;

public class HospitalRegistrationDto
{
    [Required]
    [MaxLength(200)]
    public string HospitalName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string RegistrationNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string HospitalType { get; set; } = string.Empty; // Government, Private, NGO
    
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string PinCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ContactPersonName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? ContactPersonDesignation { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string ContactEmail { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string ContactPhone { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? AlternatePhone { get; set; }
    
    [MaxLength(200)]
    public string? Website { get; set; }
    
    public bool HasBloodBank { get; set; } = false;
    public bool HasOrganTransplant { get; set; } = false;
    public bool HasEyeBank { get; set; } = false;
    public bool HasBoneMarrowRegistry { get; set; } = false;
    
    public int? BloodBankCapacity { get; set; }
    public int? BedCapacity { get; set; }
    
    [MaxLength(200)]
    public string? OperatingHours { get; set; }
    public bool IsOpen24x7 { get; set; } = false;
}
