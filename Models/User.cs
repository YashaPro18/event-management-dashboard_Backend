namespace EventManagement.API.Models
{
    public class User
    {
        public int Id { get; set; }        // PK
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "User";

        public string? PhoneNumber { get; set; }
        public string? Profession { get; set; }
        public DateTime? BirthDate { get; set; }

        public string? ProfileImageUrl { get; set; }
        public string? BannerImageUrl { get; set; }

        public string? AboutMe { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
