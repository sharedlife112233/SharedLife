using System.ComponentModel.DataAnnotations;

namespace SharedLife.Models.DTOs.Chat;

public class SendMessageDto
{
    [Required(ErrorMessage = "DonorRequestId is required")]
    public int DonorRequestId { get; set; }
    
    [Required(ErrorMessage = "Message content is required")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 1000 characters")]
    public string Content { get; set; } = string.Empty;
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public int DonorRequestId { get; set; }
    public int SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty; // "Donor" or "Recipient"
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsMine { get; set; } // Helper for frontend
}

public class ChatConversationDto
{
    public int DonorRequestId { get; set; }
    public int DonationRequestId { get; set; }
    
    // Donor info
    public int DonorUserId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    
    // Recipient info
    public int RecipientUserId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    
    // Request info
    public string BloodGroup { get; set; } = string.Empty;
    public string DonationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // Last message info
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    
    // Messages
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public class ChatListItemDto
{
    public int DonorRequestId { get; set; }
    public int OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string OtherUserRole { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string DonationType { get; set; } = string.Empty;
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
