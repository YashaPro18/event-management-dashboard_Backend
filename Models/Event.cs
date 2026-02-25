using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.API.Models
{
    public class Event
    {
        public int Id { get; set; }

        // 🔹 SECTION 1: BASIC DETAILS
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string EventType { get; set; } = string.Empty;
        // Conference, Workshop, Seminar, Meetup, Social, Blood Donation

        [Required]
        public string Status { get; set; } = "Upcoming";
        // Upcoming / Ongoing / Completed

        [Required]
        public string Description { get; set; } = string.Empty;

        // 🔹 SECTION 2: DATE & TIME
        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        // 🔹 SECTION 3: LOCATION DETAILS
        public int? VenueId { get; set; }     // Nullable for Online events
        public Venue? Venue { get; set; }

        [Required]
        public string Mode { get; set; } = "Offline";
        // Online / Offline / Hybrid

        public string? LocationOrLink { get; set; }

        // 🔹 SECTION 4: ORGANIZATION & CAPACITY
        [Required]
        public int OrganizerId { get; set; }

        [ForeignKey(nameof(OrganizerId))]
        public User? Organizer { get; set; }

        public int MaxParticipants { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0;

        public DateTime RegistrationDeadline { get; set; }

        // 🔹 SECTION 5: OPTIONAL / FUTURE
        public string? BannerImageUrl { get; set; }
        public string? Notes { get; set; }
        public bool IsPublic { get; set; } = true;

        // 🔹 SYSTEM FIELDS
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
