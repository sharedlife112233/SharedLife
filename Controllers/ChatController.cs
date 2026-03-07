using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLife.Models.DTOs.Chat;
using SharedLife.Models.Responses;
using SharedLife.Services.Interfaces;

namespace SharedLife.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return int.Parse(userIdClaim!.Value);
    }

    /// <summary>
    /// Get all chat conversations for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ChatListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChats()
    {
        var userId = GetUserId();
        var (success, message, data) = await _chatService.GetUserChatsAsync(userId);

        if (!success)
        {
            return BadRequest(ApiResponse<List<ChatListItemDto>>.ErrorResponse(message));
        }

        return Ok(ApiResponse<List<ChatListItemDto>>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Get a specific conversation with all messages
    /// </summary>
    [HttpGet("{donorRequestId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ChatConversationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ChatConversationDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ChatConversationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversation(int donorRequestId)
    {
        var userId = GetUserId();
        var (success, message, data) = await _chatService.GetConversationAsync(userId, donorRequestId);

        if (!success)
        {
            if (message.Contains("not authorized"))
            {
                return StatusCode(403, ApiResponse<ChatConversationDto>.ErrorResponse(message));
            }
            return NotFound(ApiResponse<ChatConversationDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<ChatConversationDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Send a message in a conversation
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(ApiResponse<ChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ChatMessageDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ChatMessageDto>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<ChatMessageDto>.ErrorResponse("Validation failed", errors));
        }

        var userId = GetUserId();
        var (success, message, data) = await _chatService.SendMessageAsync(userId, request);

        if (!success)
        {
            if (message.Contains("not authorized"))
            {
                return StatusCode(403, ApiResponse<ChatMessageDto>.ErrorResponse(message));
            }
            return BadRequest(ApiResponse<ChatMessageDto>.ErrorResponse(message));
        }

        return Ok(ApiResponse<ChatMessageDto>.SuccessResponse(data!, message));
    }

    /// <summary>
    /// Mark all messages in a conversation as read
    /// </summary>
    [HttpPost("{donorRequestId:int}/read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkAsRead(int donorRequestId)
    {
        var userId = GetUserId();
        var (success, message) = await _chatService.MarkMessagesAsReadAsync(userId, donorRequestId);

        if (!success)
        {
            if (message.Contains("not authorized"))
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(message));
            }
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, message));
    }

    /// <summary>
    /// Check if chat is available for a specific donor request
    /// </summary>
    [HttpGet("{donorRequestId:int}/can-chat")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CanChat(int donorRequestId)
    {
        var userId = GetUserId();
        var canChat = await _chatService.CanUserChatAsync(userId, donorRequestId);
        
        return Ok(ApiResponse<bool>.SuccessResponse(canChat, canChat ? "Chat is available" : "Chat is not available"));
    }

    /// <summary>
    /// Get unread message count for the current user
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetUserId();
            var count = await _chatService.GetUnreadCountAsync(userId);
            
            return Ok(ApiResponse<int>.SuccessResponse(count, "Unread count retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return Ok(ApiResponse<int>.SuccessResponse(0, "Unread count retrieved"));
        }
    }
}
