using SharedLife.Models.DTOs.Admin;
using SharedLife.Models.Responses;

namespace SharedLife.Services.Interfaces;

public interface IAdminService
{
    // Dashboard
    Task<(bool Success, string Message, DashboardStatsDto? Data)> GetDashboardStatsAsync();
    
    // User Management
    Task<(bool Success, string Message, UserListResponseDto? Data)> GetAllUsersAsync(int page, int pageSize, string? search, string? role);
    Task<(bool Success, string Message, UserDetailsDto? Data)> GetUserByIdAsync(int userId);
    Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserDto dto);
    Task<(bool Success, string Message)> DeleteUserAsync(int userId);
    Task<(bool Success, string Message)> VerifyUserAsync(int userId);
    
    // Donor Management
    Task<(bool Success, string Message, DonorListResponseDto? Data)> GetAllDonorsAsync(int page, int pageSize, string? search, string? bloodGroup, bool? isAvailable);
    Task<(bool Success, string Message)> VerifyDonorAsync(int donorId);
    
    // Recipient Management
    Task<(bool Success, string Message, RecipientListResponseDto? Data)> GetAllRecipientsAsync(int page, int pageSize, string? search, string? bloodGroup);
    Task<(bool Success, string Message)> VerifyRecipientAsync(int recipientId);
    
    // Request Management
    Task<(bool Success, string Message, RequestListResponseDto? Data)> GetAllRequestsAsync(int page, int pageSize, string? status, string? urgencyLevel);
    Task<(bool Success, string Message)> UpdateRequestStatusAsync(int requestId, UpdateRequestStatusDto dto);
}
