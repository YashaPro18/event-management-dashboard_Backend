using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EventManagement.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace EventManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;
        private readonly AppDbContext _context;

        public UploadController(Cloudinary cloudinary, AppDbContext context)
        {
            _cloudinary = cloudinary;
            _context = context;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(
            IFormFile file,
            [FromQuery] string type // profile | banner
        )
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // ✅ Allow only images
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only JPG and PNG images are allowed.");

            // ✅ Limit size (2MB)
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("Image size must be under 2MB.");

            // ✅ Decide folder
            var folder = type == "banner"
                ? "event/banner"
                : "event/profile";

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // 🔐 Get logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
                return NotFound("User not found");

            // ✅ Save image URL
            if (type == "profile")
                user.ProfileImageUrl = uploadResult.SecureUrl.ToString();
            else if (type == "banner")
                user.BannerImageUrl = uploadResult.SecureUrl.ToString();

            await _context.SaveChangesAsync();

            if (uploadResult.Error != null)
                return BadRequest(uploadResult.Error.Message);


            return Ok(new
            {
                url = uploadResult.SecureUrl.ToString()
            });

        }
    }
}
