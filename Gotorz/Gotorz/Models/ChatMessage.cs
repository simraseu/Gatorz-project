namespace Gotorz.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string? RecipientId { get; set; }
        public string Message { get; set; }
        public string? Destination { get; set; }
        public string? TravelGroupId { get; set; }
        public DateTime Timestamp { get; set; }
        public ChatMessageType MessageType { get; set; }
        public bool IsRead { get; set; }
    }

    public enum ChatMessageType
    {
        Private,
        DestinationGroup,
        TravelGroup
    }
}