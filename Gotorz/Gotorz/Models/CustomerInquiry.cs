using System.ComponentModel.DataAnnotations;

namespace Gotorz.Models
{
    public class CustomerInquiry
    {
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = "";

        [Required]
        public string CustomerName { get; set; } = "";

        [Required]
        public string CustomerEmail { get; set; } = "";

        public int? BookingId { get; set; }

        [Required]
        public string Subject { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        public DateTime SubmittedDate { get; set; }
        public DateTime LastUpdated { get; set; }

        public string Status { get; set; } = "Open";
        public string Priority { get; set; } = "Medium";
        public string? AssignedTo { get; set; }
        public string? Category { get; set; }
        public string? AgentReply { get; set; }
        public DateTime? ReplyDate { get; set; }
        public string? RepliedBy { get; set; }
    }
}