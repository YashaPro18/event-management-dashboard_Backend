using EventManagement.API.Data;
using EventManagement.API.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Management;
using System.Security.Claims;
namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
   
        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var isOrganizer = User.IsInRole("Organizer");
            var isAdmin = User.IsInRole("Admin");
            var isUser = User.IsInRole("User");

            var response = new DashboardResponseDto();

            // ---------------- SUMMARY ----------------
            if (isOrganizer)
            {
                response.Summary.TotalEvents = await _context.Events
                    .CountAsync(e => e.OrganizerId == userId);

                response.Summary.UpcomingEvents = await _context.Events
                    .CountAsync(e => e.OrganizerId == userId && e.Status == "Upcoming");

                response.Summary.Participants = await _context.EventRequests
                    .CountAsync(r => r.Status == "Approved" &&
                                     r.Event.OrganizerId == userId);

                response.Summary.Venues = await _context.Events
                    .Where(e => e.OrganizerId == userId && e.VenueId != null)
                    .Select(e => e.VenueId)
                    .Distinct()
                    .CountAsync();
            }
            else // Admin or User → Global
            {
                response.Summary.TotalEvents = await _context.Events.CountAsync();

                response.Summary.UpcomingEvents = await _context.Events
                    .CountAsync(e => e.Status == "Upcoming");

                response.Summary.Participants = await _context.EventRequests
                    .CountAsync(r => r.Status == "Approved");

                response.Summary.Venues = await _context.Venues.CountAsync();
            }

            // ---------------- UPCOMING EVENTS (Top 5) ----------------
            var upcomingQuery = _context.Events
                .Where(e => e.Status == "Upcoming");

            if (isOrganizer)
                upcomingQuery = upcomingQuery
                    .Where(e => e.OrganizerId == userId);

            response.UpcomingEvents = await upcomingQuery
                .OrderBy(e => e.StartDateTime)
                .Take(5)
                .Select(e => new SimpleEventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartDateTime = e.StartDateTime,
                    Status = e.Status
                })
                .ToListAsync();


            // ---------------- PENDING REQUESTS (Organizer/Admin only) ----------------
            if (isOrganizer || isAdmin)
            {
                var requestQuery = _context.EventRequests
                    .Include(r => r.Event)
                    .Include(r => r.User)
                    .Where(r => r.Status == "Pending");

                if (isOrganizer)
                    requestQuery = requestQuery
                        .Where(r => r.Event.OrganizerId == userId);

                response.PendingRequests = await requestQuery
                    .OrderByDescending(r => r.Id)
                    .Take(5)
                    .Select(r => new PendingRequestDto
                    {
                        RequestId = r.Id,
                        EventTitle = r.Event.Title,
                        UserName = r.User.Name,
                        Status = r.Status
                    })
                    .ToListAsync();
            }

            // ---------------- EVENTS PER MONTH (Bar Chart) ----------------
            var eventsQuery = _context.Events.AsQueryable();

            if (isOrganizer)
            {
                eventsQuery = eventsQuery
                    .Where(e => e.OrganizerId == userId);
            }
            response.EventsPerMonth = await eventsQuery
                .GroupBy(e => e.StartDateTime.Month)
                .Select(g => new
                {
                    MonthNumber = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.MonthNumber)
                .Select(g => new MonthlyCountDto
                {
                    Month = System.Globalization.CultureInfo.CurrentCulture
                        .DateTimeFormat.GetAbbreviatedMonthName(g.MonthNumber),
                    Count = g.Count
                })
                .ToListAsync();


            // ---------------- PARTICIPANT DISTRIBUTION (Pie Chart) ----------------
            var participantQuery = _context.EventRequests
                .Include(r => r.Event)
                .Where(r => r.Status == "Approved");

            if (isOrganizer)
                participantQuery = participantQuery
                    .Where(r => r.Event.OrganizerId == userId);

            response.ParticipantDistribution = await participantQuery
                .GroupBy(r => r.Event.EventType)
                .Select(g => new DistributionDto
                {
                    EventType = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(response);
        }
        [HttpGet("summary-ui")]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> GetUiSummary()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var events = _context.Events.Where(e => e.OrganizerId == userId);
            var eventIds = await events.Select(e => e.Id).ToListAsync();

            var approvedRequests = _context.EventRequests
                .Where(r => eventIds.Contains(r.EventId) && r.Status == "Approved");

            var pendingRequests = _context.EventRequests
                .Where(r => eventIds.Contains(r.EventId) && r.Status == "Pending");

            var totalRevenue = await approvedRequests
                .Join(_context.Events,
                    r => r.EventId,
                    e => e.Id,
                    (r, e) => (decimal?)e.Price)
                .SumAsync() ?? 0;

            var dto = new DashboardSummaryUiDto
            {
                TotalEvents = await events.CountAsync(),
                UpcomingEvents = await events.CountAsync(e => e.StartDateTime > DateTime.UtcNow),
                TotalParticipants = await approvedRequests.CountAsync(),
                TotalRevenue = totalRevenue,
                PendingRequests = await pendingRequests.CountAsync()
            };

            return Ok(dto);
        }
        [HttpGet("upcoming-ui")]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> GetUiUpcomingEvents()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var events = await _context.Events
                .Include(e => e.Venue)
                .Where(e => e.OrganizerId == userId)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            var result = new List<UpcomingEventUiDto>();

            foreach (var e in events)
            {
                var approvedCount = await _context.EventRequests
                    .CountAsync(r => r.EventId == e.Id && r.Status == "Approved");

                var percentage = e.MaxParticipants == 0
                    ? 0
                    : (approvedCount * 100) / e.MaxParticipants;

                string dashboardStatus;

                if (approvedCount >= e.MaxParticipants)
                    dashboardStatus = "FULL";
                else if (e.Status == "Upcoming")
                    dashboardStatus = "ACTIVE";
                else
                    dashboardStatus = "DRAFT";

                result.Add(new UpcomingEventUiDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    VenueName = e.Venue != null ? e.Venue.Name : "Online",
                    StartDateTime = e.StartDateTime,
                    MaxParticipants = e.MaxParticipants,
                    RegisteredCount = approvedCount,
                    Percentage = percentage,
                    DashboardStatus = dashboardStatus
                });
            }

            return Ok(result);
        }
        [HttpGet("pending-ui")]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> GetUiPendingRequests()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var requests = await _context.EventRequests
                .Include(r => r.User)
                .Include(r => r.Event)
                .Where(r => r.Event.OrganizerId == userId
                            && r.Status == "Pending")
                .OrderByDescending(r => r.RequestedAt)
                .Take(10)
                .Select(r => new PendingRequestUiDto
                {
                    RequestId = r.Id,
                    UserName = r.User.Name,
                    Email = r.User.Email,
                    EventTitle = r.Event.Title,
                    Type = r.Type,
                    RequestedAt = r.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }
    }
}
