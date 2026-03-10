using EventManagement.API.Data;
using EventManagement.API.DTOs.Events;
using EventManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace EventManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔐 MUST be logged in
    public class EventsAPIController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsAPIController(AppDbContext context)
        {
            _context = context;
        }

        

[HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventDto dto)
    {
            // 🔑 Get Organizer ID from JWT
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized();

                int organizerId = int.Parse(userIdClaim.Value);

                var eventEntity = new Event
                {
                    Title = dto.Title,
                    EventType = dto.EventType,
                    Status = dto.Status,
                    Description = dto.Description,

                    StartDateTime = DateTime.SpecifyKind(dto.StartDateTime, DateTimeKind.Utc),
                    EndDateTime = DateTime.SpecifyKind(dto.EndDateTime, DateTimeKind.Utc),
                    RegistrationDeadline = DateTime.SpecifyKind(dto.RegistrationDeadline, DateTimeKind.Utc),

                    VenueId = dto.VenueId,
                    Mode = dto.Mode,
                    LocationOrLink = dto.LocationOrLink,
                    MaxParticipants = dto.MaxParticipants,
                    Price = dto.Price,
                    IsPublic = dto.IsPublic,
                    BannerImageUrl = dto.BannerImageUrl,
                    Notes = dto.Notes,
                    OrganizerId = organizerId,

                    CreatedAt = DateTime.UtcNow
                };

                _context.Events.Add(eventEntity);
                await _context.SaveChangesAsync();

                return Ok(eventEntity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //if (userIdClaim == null)
        //    return Unauthorized();

        //int organizerId = int.Parse(userIdClaim.Value);

        //// ✅ MAP DTO → ENTITY (THIS IS THE FIX)
        //var eventEntity = new Event
        //{
        //    Title = dto.Title,
        //    EventType = dto.EventType,
        //    Status = dto.Status,
        //    Description = dto.Description,

        //    StartDateTime = dto.StartDateTime,
        //    EndDateTime = dto.EndDateTime,
        //    RegistrationDeadline = dto.RegistrationDeadline,

        //    VenueId = dto.VenueId,
        //    Mode = dto.Mode,
        //    LocationOrLink = dto.LocationOrLink,

        //    MaxParticipants = dto.MaxParticipants,
        //    Price = dto.Price,
        //    IsPublic = dto.IsPublic,

        //    BannerImageUrl = dto.BannerImageUrl,
        //    Notes = dto.Notes,

        //    OrganizerId = organizerId,     // 🔒 forced from JWT
        //    CreatedAt = DateTime.UtcNow
        //};

        //_context.Events.Add(eventEntity);
        //await _context.SaveChangesAsync();

        //return Ok(eventEntity);

        // GET: api/EventsAPI
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(events);
        }

        //ADD GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventEntity == null)
                return NotFound();

            return Ok(eventEntity);
        }
        //eaditing, also add PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, Event eventModel)
        {
            if (id != eventModel.Id)
                return BadRequest("Event ID mismatch");

            // 🔑 Organizer from JWT
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            eventModel.OrganizerId = int.Parse(userIdClaim.Value);

            // 🔒 Prevent EF from inserting related entities
            _context.Entry(eventModel).Reference(e => e.Organizer).IsModified = false;

            if (eventModel.VenueId.HasValue)
            {
                _context.Entry(eventModel).Reference(e => e.Venue).IsModified = false;
            }

            eventModel.UpdatedAt = DateTime.UtcNow;

            _context.Events.Update(eventModel);
            await _context.SaveChangesAsync();

            return Ok(eventModel);
        }

        // DELETE: api/EventsAPI/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
                return NotFound("Event not found");

            // 🔐 Extra safety: organizer can delete only own events
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (User.IsInRole("Organizer") && ev.OrganizerId != userId)
                return Forbid("You can delete only your own events");

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully" });
        }

    }
}
