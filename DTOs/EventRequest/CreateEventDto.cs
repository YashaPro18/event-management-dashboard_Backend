namespace EventManagement.API.DTOs.Events
{
    public class CreateEventDto
    {
        public required string Title { get; set; }
        public required string EventType { get; set; }
        public required string Status { get; set; }
        public required string Description { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime RegistrationDeadline { get; set; }

        public int? VenueId { get; set; }
        public required string Mode { get; set; }
        public string? LocationOrLink { get; set; }

        public int MaxParticipants { get; set; }
        public decimal Price { get; set; }
        public bool IsPublic { get; set; }

        public string? BannerImageUrl { get; set; }
        public string? Notes { get; set; }
    }
}
