using Gotorz.Data;
using Gotorz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gotorz.Services
{
    public interface ICustomerMessageService
    {
        Task<CustomerMessage> SendMessageAsync(string senderId, string recipientEmail, string subject, string message, MessageType messageType, int? bookingId = null);
        Task<CustomerMessage> SendPasswordResetAsync(string senderId, string recipientEmail, string tempPassword);
        Task<List<CustomerMessage>> GetMessagesForCustomerAsync(string customerEmail);
        Task<List<CustomerMessage>> GetSentMessagesAsync(string senderId);
        Task<bool> MarkAsReadAsync(int messageId, string customerEmail);
        Task<CustomerMessage?> GetMessageByIdAsync(int messageId);
        Task<int> GetUnreadCountAsync(string customerEmail);
    }

    public class CustomerMessageService : ICustomerMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CustomerMessageService> _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerMessageService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CustomerMessageService> logger,
            IActivityLogService activityLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _activityLogService = activityLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CustomerMessage> SendMessageAsync(string senderId, string recipientEmail, string subject, string message, MessageType messageType, int? bookingId = null)
        {
            try
            {
                // Get sender info
                var sender = await _userManager.FindByIdAsync(senderId);
                if (sender == null)
                {
                    throw new InvalidOperationException($"Sender with ID {senderId} not found");
                }

                // Get recipient info
                var recipient = await _userManager.FindByEmailAsync(recipientEmail);
                if (recipient == null)
                {
                    throw new InvalidOperationException($"Recipient with email {recipientEmail} not found");
                }

                var customerMessage = new CustomerMessage
                {
                    SenderId = senderId,
                    SenderName = $"{sender.FirstName} {sender.LastName}",
                    RecipientId = recipient.Id,
                    RecipientEmail = recipientEmail,
                    Subject = subject,
                    Message = message,
                    MessageType = messageType,
                    RelatedBookingId = bookingId,
                    SentDate = DateTime.UtcNow,
                    Priority = messageType == MessageType.PasswordReset ? MessagePriority.High : MessagePriority.Normal
                };

                _context.CustomerMessages.Add(customerMessage);
                await _context.SaveChangesAsync();

                // Log the activity
                await _activityLogService.LogActivityAsync(
                    senderId,
                    "Message Sent",
                    $"Sent {messageType} message to {recipientEmail}: {subject}",
                    _httpContextAccessor.HttpContext
                );

                _logger.LogInformation($"Message sent from {sender.Email} to {recipientEmail}: {subject}");

                return customerMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                throw;
            }
        }

        public async Task<CustomerMessage> SendPasswordResetAsync(string senderId, string recipientEmail, string tempPassword)
        {
            var subject = "Your Password Has Been Reset - Gotorz";
            var message = $@"Hello,

Your password has been reset by our administrator.

Your new temporary password is: {tempPassword}

Please log in with this temporary password and change it immediately in your account settings.

For security reasons, this temporary password will expire in 24 hours.

If you did not request this password reset, please contact our support team immediately.

Best regards,
Gotorz Support Team";

            var customerMessage = await SendMessageAsync(senderId, recipientEmail, subject, message, MessageType.PasswordReset);

            // Store encrypted temp password for audit trail
            customerMessage.TempPassword = tempPassword; // In production, this should be encrypted
            await _context.SaveChangesAsync();

            return customerMessage;
        }

        public async Task<List<CustomerMessage>> GetMessagesForCustomerAsync(string customerEmail)
        {
            try
            {
                return await _context.CustomerMessages
                    .Where(m => m.RecipientEmail == customerEmail)
                    .OrderByDescending(m => m.SentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting messages for customer {customerEmail}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CustomerMessage>> GetSentMessagesAsync(string senderId)
        {
            try
            {
                return await _context.CustomerMessages
                    .Where(m => m.SenderId == senderId)
                    .OrderByDescending(m => m.SentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting sent messages for sender {senderId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(int messageId, string customerEmail)
        {
            try
            {
                var message = await _context.CustomerMessages
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.RecipientEmail == customerEmail);

                if (message == null)
                {
                    return false;
                }

                if (!message.IsRead)
                {
                    message.IsRead = true;
                    message.ReadDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking message {messageId} as read: {ex.Message}");
                throw;
            }
        }

        public async Task<CustomerMessage?> GetMessageByIdAsync(int messageId)
        {
            try
            {
                return await _context.CustomerMessages
                    .FirstOrDefaultAsync(m => m.Id == messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting message {messageId}: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(string customerEmail)
        {
            try
            {
                return await _context.CustomerMessages
                    .CountAsync(m => m.RecipientEmail == customerEmail && !m.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting unread count for {customerEmail}: {ex.Message}");
                throw;
            }
        }
    }
}