using SharedLife.Models.DTOs.Hospital;
using SharedLife.Models.DTOs.Recipient;

namespace SharedLife.Services.Interfaces;

public interface IHospitalService
{
    // Hospital Profile Management
    Task<(bool Success, string Message, HospitalProfileDto? Data)> RegisterHospitalAsync(int userId, HospitalRegistrationDto request);
    Task<(bool Success, string Message, HospitalProfileDto? Data)> GetHospitalProfileAsync(int userId);
    Task<(bool Success, string Message, HospitalProfileDto? Data)> GetHospitalByIdAsync(int hospitalId);
    Task<(bool Success, string Message, HospitalProfileDto? Data)> UpdateHospitalAsync(int userId, HospitalUpdateDto request);
    Task<bool> IsHospitalAsync(int userId);
    
    // Dashboard
    Task<(bool Success, string Message, HospitalDashboardDto? Data)> GetDashboardAsync(int userId);
    
    // Donor Verification
    Task<(bool Success, string Message, List<PendingDonorDto>? Data)> GetPendingDonorsAsync(int userId, int page, int pageSize);
    Task<(bool Success, string Message)> VerifyDonorAsync(int userId, DonorVerificationDto request);
    Task<(bool Success, string Message, List<PendingDonorDto>? Data)> GetVerifiedDonorsAsync(int userId, int page, int pageSize);
    
    // Donation Requests
    Task<(bool Success, string Message, List<DonationRequestDto>? Data)> GetAreaRequestsAsync(int userId, int page, int pageSize, string? status);
    Task<(bool Success, string Message)> ProcessRequestAsync(int userId, int requestId, string action, string? notes);
    
    // Donor Offers
    Task<(bool Success, string Message, List<Models.DTOs.Donor.DonorOfferDto>? Data)> GetAreaDonorOffersAsync(int userId, int page, int pageSize);
}
