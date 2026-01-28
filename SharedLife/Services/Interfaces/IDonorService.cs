using SharedLife.Models.DTOs.Donor;
using SharedLife.Models.Entities;

namespace SharedLife.Services.Interfaces;

public interface IDonorService
{
    Task<(bool Success, string Message, DonorProfileDto? Data)> RegisterDonorAsync(int userId, DonorRegistrationDto request);
    Task<(bool Success, string Message, DonorProfileDto? Data)> GetDonorProfileAsync(int userId);
    Task<(bool Success, string Message, DonorProfileDto? Data)> GetDonorByIdAsync(int donorId);
    Task<(bool Success, string Message, DonorProfileDto? Data)> UpdateDonorAsync(int userId, DonorUpdateDto request);
    Task<(bool Success, string Message)> UpdateAvailabilityAsync(int userId, DonorAvailabilityDto request);
    Task<(bool Success, string Message, List<DonorListItemDto>? Data)> GetAllDonorsAsync(int page, int pageSize, string? bloodGroup, bool? isAvailable);
    Task<(bool Success, string Message)> RecordBloodDonationAsync(int userId, DateTime donationDate);
    Task<bool> IsDonorAsync(int userId);
}
