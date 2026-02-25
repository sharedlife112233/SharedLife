using SharedLife.Models.DTOs.Chat;

namespace SharedLife.Services.Interfaces;

public interface IChatService
{
    /// <summary>
    /// Send a message in a chat conversation
    /// </summary>
    Task<(bool Success, string Message, ChatMessageDto? Data)> SendMessageAsync(int senderUserId, SendMessageDto request);
    
    /// <summary>
    /// Get all messages for a specific donor request conversation
    /// </summary>
    Task<(bool Success, string Message, ChatConversationDto? Data)> GetConversationAsync(int userId, int donorRequestId);
    
    /// <summary>
    /// Get all chat conversations for a user (donor or recipient)
    /// </summary>
    Task<(bool Success, string Message, List<ChatListItemDto>? Data)> GetUserChatsAsync(int userId);
    
    /// <summary>
    /// Mark messages as read
    /// </summary>
    Task<(bool Success, string Message)> MarkMessagesAsReadAsync(int userId, int donorRequestId);
    
    /// <summary>
    /// Check if user can chat in a specific donor request
    /// </summary>
    Task<bool> CanUserChatAsync(int userId, int donorRequestId);
    
    /// <summary>
    /// Get unread message count for a user
    /// </summary>
    Task<int> GetUnreadCountAsync(int userId);
}
