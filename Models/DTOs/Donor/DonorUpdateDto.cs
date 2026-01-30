using System.ComponentModel.DataAnnotations;
using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Donor;

public class DonorUpdateDto
{
    [Range(30, 200, ErrorMessage = "Weight must be between 30 and 200 kg")]
    public double? Weight { get; set; }
    
    [Range(100, 250, ErrorMessage = "Height must be between 100 and 250 cm")]
    public double? Height { get; set; }
    
    // Donation Preferences
    public bool WillingToDonatePlasma { get; set; }
    public bool WillingToDonatePlatelets { get; set; }
    public bool WillingToDonateOrgan { get; set; }
    public bool WillingToDonateBoneMarrow { get; set; }
    public bool WillingToDonateEye { get; set; }
    
    public List<OrganType>? PledgedOrgans { get; set; }
    
    // Health Information
    public bool HasChronicDisease { get; set; }
    public string? ChronicDiseaseDetails { get; set; }
    public bool HasInfectiousDisease { get; set; }
    public bool IsSmoker { get; set; }
    public bool ConsumesAlcohol { get; set; }
    public string? Medications { get; set; }
    public string? Allergies { get; set; }
    
    // Availability
    public bool IsAvailable { get; set; }
    public string? AvailabilityNotes { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
}
