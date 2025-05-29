namespace Gatorz.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string IPAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserAgent { get; set; }
    }
}