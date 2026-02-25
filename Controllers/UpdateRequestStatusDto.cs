namespace EventManagement.API.DTOs.EventRequest
{
    public class UpdateRequestStatusDto
    {
        public required string Status { get; set; } // Approved | Rejected
    }
}
