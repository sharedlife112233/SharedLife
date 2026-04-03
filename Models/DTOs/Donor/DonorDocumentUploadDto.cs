using System.ComponentModel.DataAnnotations;

namespace SharedLife.Models.DTOs.Donor;

public class DonorDocumentUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
