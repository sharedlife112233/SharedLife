using System.ComponentModel.DataAnnotations;

namespace SharedLife.Models.DTOs.Hospital;

public class HospitalUpdateDto
{
    [MaxLength(200)]
    public string? HospitalName { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(100)]
    public string? State { get; set; }
    
    [MaxLength(10)]
    public string? PinCode { get; set; }
    
    [EmailAddress]
    [MaxLength(255)]
    public string? ContactEmail { get; set; }
    
    [MaxLength(20)]
    public string? ContactPhone { get; set; }
    
    [MaxLength(20)]
    public string? AlternatePhone { get; set; }
    
    [MaxLength(200)]
    public string? Website { get; set; }
    
    public bool? HasBloodBank { get; set; }
    public bool? HasOrganTransplant { get; set; }
    public bool? HasEyeBank { get; set; }
    public bool? HasBoneMarrowRegistry { get; set; }
    
    public int? BloodBankCapacity { get; set; }
    public int? BedCapacity { get; set; }
    
    [MaxLength(200)]
    public string? OperatingHours { get; set; }
    public bool? IsOpen24x7 { get; set; }
}
