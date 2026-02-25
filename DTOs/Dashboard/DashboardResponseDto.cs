namespace EventManagement.API.DTOs.Dashboard
{
    public class DashboardResponseDto
    {
        public SummaryDto Summary { get; set; } = new();

        public List<SimpleEventDto> UpcomingEvents { get; set; } = new();

        public List<PendingRequestDto> PendingRequests { get; set; } = new();

        public List<MonthlyCountDto> EventsPerMonth { get; set; } = new();

        public List<DistributionDto> ParticipantDistribution { get; set; } = new();
    }

    public class SummaryDto
    {
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int Participants { get; set; }
        public int Venues { get; set; }
    }

    public class SimpleEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime StartDateTime { get; set; }
        public string Status { get; set; } = "";
    }

    public class PendingRequestDto
    {
        public int RequestId { get; set; }
        public string EventTitle { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class MonthlyCountDto
    {
        public string Month { get; set; } = "";
        public int Count { get; set; }
    }

    public class DistributionDto
    {
        public string EventType { get; set; } = "";
        public int Count { get; set; }
    }
}
