namespace EventManagement.API.DTOs.EventRequest
{
    public class CreateEventRequestDto
    {
        public int EventId { get; set; }
        public string Type { get; set; } = ""; // Attend | Volunteer
    }
}
