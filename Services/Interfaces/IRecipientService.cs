using SharedLife.Models.DTOs.Recipient;
using SharedLife.Models.Enums;

namespace SharedLife.Services.Interfaces;

public interface IRecipientService
{
    // Recipient Profile
    Task<(bool Success, string Message, RecipientProfileDto? Data)> CreateRecipientProfileAsync(int userId, RecipientRegistrationDto request);
    Task<(bool Success, string Message, RecipientProfileDto? Data)> GetRecipientProfileAsync(int userId);
    Task<(bool Success, string Message, RecipientProfileDto? Data)> UpdateRecipientProfileAsync(int userId, RecipientRegistrationDto request);
    Task<(bool Success, string Message, RecipientStatusDto? Data)> CheckRecipientStatusAsync(int userId);
    
    // Donation Requests
    Task<(bool Success, string Message, DonationRequestDto? Data)> CreateDonationRequestAsync(int userId, CreateDonationRequestDto request);
    Task<(bool Success, string Message, List<DonationRequestDto>? Data)> GetUserDonationRequestsAsync(int userId);
    Task<(bool Success, string Message, DonationRequestDto? Data)> GetDonationRequestByIdAsync(int userId, int requestId);
    Task<(bool Success, string Message)> CancelDonationRequestAsync(int userId, int requestId);
    Task<(bool Success, string Message, DonationRequestDto? Data)> UpdateDonationRequestStatusAsync(int userId, int requestId, RequestStatus status);
    
    // Matching Donors
    Task<(bool Success, string Message, List<MatchingDonorDto>? Data)> FindMatchingDonorsAsync(BloodGroup bloodGroup, DonationType donationType, string? city = null);
    Task<(bool Success, string Message, int MatchedCount)> DistributeRequestToDonorsAsync(int userId, int requestId);
    
    // Donor History
    Task<(bool Success, string Message, List<DonorHistoryDto>? Data)> GetDonorHistoryAsync(int userId);
    
    // Stats
    Task<(bool Success, string Message, object? Data)> GetRecipientDashboardStatsAsync(int userId);
}
