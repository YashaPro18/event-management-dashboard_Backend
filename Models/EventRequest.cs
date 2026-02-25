using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.API.Models
{
    public class EventRequest
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Attend | Volunteer
        [Required]
        public string Type { get; set; } = string.Empty;

        // Pending | Approved | Rejected
        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}
