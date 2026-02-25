namespace EventManagement.API.DTOs.Dashboard
{
    public class DashboardSummaryUiDto
    {
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int TotalParticipants { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingRequests { get; set; }
    }

    public class UpcomingEventUiDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string VenueName { get; set; } = "";
        public DateTime StartDateTime { get; set; }
        public int MaxParticipants { get; set; }
        public int RegisteredCount { get; set; }
        public int Percentage { get; set; }
        public string DashboardStatus { get; set; } = "";
    }

    public class PendingRequestUiDto
    {
        public int RequestId { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string EventTitle { get; set; } = "";
        public string Type { get; set; } = "";
        public DateTime RequestedAt { get; set; }
    }
}