using System.ComponentModel.DataAnnotations;
using SharedLife.Models.Enums;

namespace SharedLife.Models.DTOs.Hospital;

public class DonorVerificationDto
{
    [Required]
    public int DonorId { get; set; }
    
    [Required]
    public DonorStatus NewStatus { get; set; }
    
    [MaxLength(500)]
    public string? VerificationNotes { get; set; }
}
