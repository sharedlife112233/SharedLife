using Microsoft.EntityFrameworkCore;
using SharedLife.Data;
using SharedLife.Models.DTOs.Chat;
using SharedLife.Models.Entities;
using SharedLife.Models.Enums;
using SharedLife.Services.Interfaces;
using SharedLife.Utilities;

namespace SharedLife.Services;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChatService> _logger;

    public ChatService(ApplicationDbContext context, ILogger<ChatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, ChatMessageDto? Data)> SendMessageAsync(int senderUserId, SendMessageDto request)
    {
        try
        {
            // Check if user can chat
            if (!await CanUserChatAsync(senderUserId, request.DonorRequestId))
            {
                return (false, "You are not authorized to send messages in this conversation", null);
            }

            var donorRequest = await _context.DonorRequests
                .Include(dr => dr.Donor)
                    .ThenInclude(d => d.User)
                .Include(dr => dr.DonationRequest)
                    .ThenInclude(dr => dr.Recipient)
                        .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(dr => dr.Id == request.DonorRequestId);

            if (donorRequest == null)
            {
                return (false, "Conversation not found", null);
            }

            var senderUser = await _context.Users.FindAsync(senderUserId);
            if (senderUser == null)
            {
                return (false, "Sender not found", null);
            }

            var message = new ChatMessage
            {
                DonorRequestId = request.DonorRequestId,
                SenderUserId = senderUserId,
                Content = request.Content.Trim(),
                IsRead = false,
                CreatedAt = TimeHelper.Now
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            var senderRole = senderUserId == donorRequest.Donor.UserId ? "Donor" : "Recipient";

            var messageDto = new ChatMessageDto
            {
                Id = message.Id,
                DonorRequestId = message.DonorRequestId,
                SenderUserId = message.SenderUserId,
                SenderName = senderUser.FullName,
                SenderRole = senderRole,
                Content = message.Content,
                IsRead = message.IsRead,
                ReadAt = message.ReadAt,
                CreatedAt = message.CreatedAt,
                IsMine = true
            };

            _logger.LogInformation("Message sent by user {UserId} in conversation {DonorRequestId}", senderUserId, request.DonorRequestId);
            return (true, "Message sent successfully", messageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return (false, "An error occurred while sending the message", null);
        }
    }

    public async Task<(bool Success, string Message, ChatConversationDto? Data)> GetConversationAsync(int userId, int donorRequestId)
    {
        try
        {
            // Check if user can access this conversation
            if (!await CanUserChatAsync(userId, donorRequestId))
            {
                return (false, "You are not authorized to access this conversation", null);
            }

            var donorRequest = await _context.DonorRequests
                .Include(dr => dr.Donor)
                    .ThenInclude(d => d.User)
                .Include(dr => dr.DonationRequest)
                    .ThenInclude(d => d.Recipient)
                        .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(dr => dr.Id == donorRequestId);

            if (donorRequest == null)
            {
                return (false, "Conversation not found", null);
            }

            var messages = await _context.ChatMessages
                .Include(m => m.SenderUser)
                .Where(m => m.DonorRequestId == donorRequestId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            var donorUserId = donorRequest.Donor.UserId;
            var recipientUserId = donorRequest.DonationRequest.Recipient.UserId;

            var conversation = new ChatConversationDto
            {
                DonorRequestId = donorRequestId,
                DonationRequestId = donorRequest.DonationRequestId,
                DonorUserId = donorUserId,
                DonorName = donorRequest.Donor.User.FullName,
                RecipientUserId = recipientUserId,
                RecipientName = donorRequest.DonationRequest.Recipient.User.FullName,
                BloodGroup = donorRequest.DonationRequest.BloodGroup.ToString(),
                DonationType = donorRequest.DonationRequest.DonationType.ToString(),
                Status = donorRequest.Status.ToString(),
                LastMessage = messages.LastOrDefault()?.Content,
                LastMessageAt = messages.LastOrDefault()?.CreatedAt,
                UnreadCount = messages.Count(m => !m.IsRead && m.SenderUserId != userId),
                Messages = messages.Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    DonorRequestId = m.DonorRequestId,
                    SenderUserId = m.SenderUserId,
                    SenderName = m.SenderUser.FullName,
                    SenderRole = m.SenderUserId == donorUserId ? "Donor" : "Recipient",
                    Content = m.Content,
                    IsRead = m.IsRead,
                    ReadAt = m.ReadAt,
                    CreatedAt = m.CreatedAt,
                    IsMine = m.SenderUserId == userId
                }).ToList()
            };

            return (true, "Conversation retrieved successfully", conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation");
            return (false, "An error occurred while retrieving the conversation", null);
        }
    }

    public async Task<(bool Success, string Message, List<ChatListItemDto>? Data)> GetUserChatsAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User not found", null);
            }

            // Find all accepted donor requests where user is involved (as donor or recipient)
            List<DonorRequest> donorRequests;

            if (user.Role == UserRole.Donor)
            {
                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (donor == null)
                {
                    return (true, "No conversations found", new List<ChatListItemDto>());
                }

                donorRequests = await _context.DonorRequests
                    .Include(dr => dr.Donor)
                        .ThenInclude(d => d.User)
                    .Include(dr => dr.DonationRequest)
                        .ThenInclude(d => d.Recipient)
                            .ThenInclude(r => r.User)
                    .Where(dr => dr.DonorId == donor.Id && dr.Status == RequestStatus.Accepted)
                    .ToListAsync();
            }
            else if (user.Role == UserRole.Recipient)
            {
                var recipient = await _context.Recipients.FirstOrDefaultAsync(r => r.UserId == userId);
                if (recipient == null)
                {
                    return (true, "No conversations found", new List<ChatListItemDto>());
                }

                donorRequests = await _context.DonorRequests
                    .Include(dr => dr.Donor)
                        .ThenInclude(d => d.User)
                    .Include(dr => dr.DonationRequest)
                        .ThenInclude(d => d.Recipient)
                            .ThenInclude(r => r.User)
                    .Where(dr => dr.DonationRequest.RecipientId == recipient.Id && dr.Status == RequestStatus.Accepted)
                    .ToListAsync();
            }
            else
            {
                return (false, "Only donors and recipients can access chats", null);
            }

            var chatList = new List<ChatListItemDto>();

            foreach (var dr in donorRequests)
            {
                var lastMessage = await _context.ChatMessages
                    .Where(m => m.DonorRequestId == dr.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                var unreadCount = await _context.ChatMessages
                    .CountAsync(m => m.DonorRequestId == dr.Id && !m.IsRead && m.SenderUserId != userId);

                var isUserDonor = dr.Donor.UserId == userId;
                var otherUser = isUserDonor 
                    ? dr.DonationRequest.Recipient.User 
                    : dr.Donor.User;

                chatList.Add(new ChatListItemDto
                {
                    DonorRequestId = dr.Id,
                    OtherUserId = otherUser.Id,
                    OtherUserName = otherUser.FullName,
                    OtherUserRole = isUserDonor ? "Recipient" : "Donor",
                    BloodGroup = dr.DonationRequest.BloodGroup.ToString(),
                    DonationType = dr.DonationRequest.DonationType.ToString(),
                    LastMessage = lastMessage?.Content,
                    LastMessageAt = lastMessage?.CreatedAt,
                    UnreadCount = unreadCount
                });
            }

            // Sort by last message time
            chatList = chatList.OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue).ToList();

            return (true, "Chats retrieved successfully", chatList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user chats");
            return (false, "An error occurred while retrieving chats", null);
        }
    }

    public async Task<(bool Success, string Message)> MarkMessagesAsReadAsync(int userId, int donorRequestId)
    {
        try
        {
            if (!await CanUserChatAsync(userId, donorRequestId))
            {
                return (false, "You are not authorized to access this conversation");
            }

            var unreadMessages = await _context.ChatMessages
                .Where(m => m.DonorRequestId == donorRequestId && !m.IsRead && m.SenderUserId != userId)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadAt = TimeHelper.Now;
            }

            await _context.SaveChangesAsync();

            return (true, $"{unreadMessages.Count} messages marked as read");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read");
            return (false, "An error occurred while marking messages as read");
        }
    }

    public async Task<bool> CanUserChatAsync(int userId, int donorRequestId)
    {
        var donorRequest = await _context.DonorRequests
            .Include(dr => dr.Donor)
            .Include(dr => dr.DonationRequest)
                .ThenInclude(d => d.Recipient)
            .FirstOrDefaultAsync(dr => dr.Id == donorRequestId);

        if (donorRequest == null)
        {
            return false;
        }

        // Only allow chat if donor has accepted the request
        if (donorRequest.Status != RequestStatus.Accepted)
        {
            return false;
        }

        // Check if user is the donor or the recipient
        var donorUserId = donorRequest.Donor.UserId;
        var recipientUserId = donorRequest.DonationRequest.Recipient.UserId;

        return userId == donorUserId || userId == recipientUserId;
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return 0;

            // Get all donor request IDs where user can chat
            List<int> donorRequestIds;

            if (user.Role == UserRole.Donor)
            {
                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (donor == null) return 0;

                donorRequestIds = await _context.DonorRequests
                    .Where(dr => dr.DonorId == donor.Id && dr.Status == RequestStatus.Accepted)
                    .Select(dr => dr.Id)
                    .ToListAsync();
            }
            else if (user.Role == UserRole.Recipient)
            {
                var recipient = await _context.Recipients.FirstOrDefaultAsync(r => r.UserId == userId);
                if (recipient == null) return 0;

                donorRequestIds = await _context.DonorRequests
                    .Where(dr => dr.DonationRequest.RecipientId == recipient.Id && dr.Status == RequestStatus.Accepted)
                    .Select(dr => dr.Id)
                    .ToListAsync();
            }
            else
            {
                return 0;
            }

            if (!donorRequestIds.Any()) return 0;

            // Convert to HashSet for fast lookup and count client-side
            // to avoid Pomelo/MySQL primitive collection translation issues
            var donorRequestIdSet = new HashSet<int>(donorRequestIds);
            var unreadCount = await _context.ChatMessages
                .Where(m => !m.IsRead && m.SenderUserId != userId)
                .Select(m => m.DonorRequestId)
                .ToListAsync();

            return unreadCount.Count(id => donorRequestIdSet.Contains(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
            return 0;
        }
    }
}
