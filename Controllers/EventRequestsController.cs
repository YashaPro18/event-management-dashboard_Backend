
using EventManagement.API.Data;
using EventManagement.API.DTOs.EventRequest;
using EventManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/EventRequests
        [HttpPost]
        public async Task<IActionResult> CreateRequest(CreateEventRequestDto dto)
        {
            // 🔐 Get logged-in user id
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // ❌ Prevent duplicate request
            var exists = await _context.EventRequests
                .AnyAsync(r =>
                    r.EventId == dto.EventId &&
                    r.UserId == userId &&
                    r.Type == dto.Type);

            if (exists)
            {
                return BadRequest(new { message = "You already sent this request." });

            }

            var request = new EventRequest
            {
                EventId = dto.EventId,
                UserId = userId,
                Type = dto.Type,               // Attend | Volunteer
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _context.EventRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request sent successfully" });

        }

        // GET: api/EventRequests/my-events
        [HttpGet("my-events")]
        [Authorize]
        public async Task<IActionResult> GetRequestsForMyEvents()
        {
            // 🔐 Logged-in USER id
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var requests = await _context.EventRequests
                .Include(r => r.Event)
                .Where(r => r.UserId == userId)   // ✅ THIS IS THE KEY LINE
                .Select(r => new
                {
                    r.Id,
                    EventId = r.EventId,
                    EventTitle = r.Event.Title,
                    r.Type,
                    r.Status,
                    r.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        //Organizer sees only their events’ requests
        //Event name is included
        //Safe, production-correct filtering

        // GET: api/EventRequests/organizer
        [HttpGet("organizer")]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> GetRequestsForOrganizer()
        {
            // 🔑 Logged-in organizer id
            var organizerId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var requests = await _context.EventRequests
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.Event.OrganizerId == organizerId)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new EventRequestResponseDto
                {
                    Id = r.Id,
                    EventId = r.EventId,
                    EventTitle = r.Event.Title,

                    UserId = r.UserId,
                    UserName = r.User.Name,

                    Type = r.Type,
                    Status = r.Status,
                    RequestedAt = r.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> UpdateStatus(
    int id,
    UpdateRequestStatusDto dto)
        {
            if (dto.Status != "Approved" && dto.Status != "Rejected")
                return BadRequest("Invalid status");

            var request = await _context.EventRequests
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound("Request not found");

            // 🔐 Logged-in user
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            // 🔒 Organizer can approve ONLY their own event requests
            if (User.IsInRole("Organizer") &&
                request.Event.OrganizerId != userId)
            {
                return Forbid("You cannot manage requests for this event");
            }

            request.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Request {dto.Status}",
                requestId = request.Id
            });
        }


    }
}
