namespace EventManagement.API.DTOs
{
    public class UpdateProfileRequest
    {
        public string? PhoneNumber { get; set; }
        public string? Profession { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? AboutMe { get; set; }
    }
}
