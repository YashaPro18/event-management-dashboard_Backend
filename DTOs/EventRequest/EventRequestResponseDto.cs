namespace EventManagement.API.DTOs.EventRequest
{
    public class EventRequestResponseDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = "";

        public int UserId { get; set; }
        public string UserName { get; set; } = "";

        public string Type { get; set; } = "";   // Attend | Volunteer
        public string Status { get; set; } = ""; // Pending | Approved | Rejected

        public DateTime RequestedAt { get; set; }
    }
}
