using System.ComponentModel.DataAnnotations;

namespace Gotorz.Models
{
    public class CustomerMessage
    {
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; } = ""; // Admin or SalesAgent user ID

        [Required]
        public string SenderName { get; set; } = ""; // Display name

        [Required]
        public string RecipientId { get; set; } = ""; // Customer user ID (ApplicationUser.Id)

        [Required]
        public string RecipientEmail { get; set; } = ""; // Customer email for easy lookup

        [Required]
        public string Subject { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        [Required]
        public MessageType MessageType { get; set; } = MessageType.General;

        public int? RelatedBookingId { get; set; } // Optional - for booking-related messages

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadDate { get; set; }

        public MessagePriority Priority { get; set; } = MessagePriority.Normal;

        // For password reset messages
        public string? TempPassword { get; set; } = null; // Encrypted temp password
    }

    public enum MessageType
    {
        General,
        PasswordReset,
        BookingChange,
        BookingConfirmation,
        Support,
        SystemNotification
    }

    public enum MessagePriority
    {
        Low,
        Normal,
        High,
        Urgent
    }
}