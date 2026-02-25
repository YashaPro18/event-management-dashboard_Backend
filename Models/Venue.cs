namespace EventManagement.API.Models
{
    public class Venue
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public bool IsOnline { get; set; }  // true = online venue
    }
}
