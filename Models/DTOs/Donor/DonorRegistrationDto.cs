using System.ComponentModel.DataAnnotations;
using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Donor;

public class DonorRegistrationDto
{
    [Required(ErrorMessage = "Blood group is required")]
    public BloodGroup BloodGroup { get; set; }
    
    [Range(30, 200, ErrorMessage = "Weight must be between 30 and 200 kg")]
    public double? Weight { get; set; }
    
    [Range(100, 250, ErrorMessage = "Height must be between 100 and 250 cm")]
    public double? Height { get; set; }
    
    // Donation Preferences
    public bool WillingToDonateBlood { get; set; } = false;
    public bool WillingToDonatePlasma { get; set; } = false;
    public bool WillingToDonatePlatelets { get; set; } = false;
    public bool WillingToDonateOrgan { get; set; } = false;
    public bool WillingToDonateBoneMarrow { get; set; } = false;
    public bool WillingToDonateEye { get; set; } = false;
    
    // Organ Pledge (list of organ types)
    public List<OrganType>? PledgedOrgans { get; set; }
    
    // Health Information
    public bool HasChronicDisease { get; set; } = false;
    public string? ChronicDiseaseDetails { get; set; }
    public bool HasInfectiousDisease { get; set; } = false;
    public bool IsSmoker { get; set; } = false;
    public bool ConsumesAlcohol { get; set; } = false;
    public string? Medications { get; set; }
    public string? Allergies { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
}
