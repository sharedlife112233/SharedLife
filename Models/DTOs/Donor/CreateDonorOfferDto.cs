using System.ComponentModel.DataAnnotations;
using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Donor;

public class CreateDonorOfferDto
{
    [Required]
    public DonationType DonationType { get; set; }
    
    [Range(1, 10)]
    public int Quantity { get; set; } = 1;
    
    [Required]
    [MaxLength(200)]
    public string HospitalName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string HospitalLocation { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    public DateTime PreferredDate { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}
