using SharedLife.Utilities;

namespace SharedLife.Models.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    
    // The DonorRequest that enabled this chat (must be Accepted status)
    public int DonorRequestId { get; set; }
    public virtual DonorRequest DonorRequest { get; set; } = null!;
    
    // Sender (User Id - can be either Donor's User or Recipient's User)
    public int SenderUserId { get; set; }
    public virtual User SenderUser { get; set; } = null!;
    
    // Message content
    public string Content { get; set; } = string.Empty;
    
    // Read status
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;
}
