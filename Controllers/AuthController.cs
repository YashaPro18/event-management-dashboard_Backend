using EventManagement.API.Data;
using EventManagement.API.DTOs;
using EventManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FirebaseAdmin.Auth;



namespace EventManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        


        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var allowedRoles = new[] { "User", "Organizer", "Admin" };

            if (!allowedRoles.Contains(request.Role))
            {
                return BadRequest("Invalid role selected");
            }
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already exists");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = request.Role
            };


            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }


        // ---------------- LOGIN ----------------
        
        [HttpPost("firebase-login")]
        public async Task<IActionResult> FirebaseLogin(FirebaseLoginRequest request)
        {
            FirebaseToken decodedToken;
        
            try
            {
                decodedToken = await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(request.IdToken);
            }
            catch
            {
                return Unauthorized("Invalid Firebase token");
            }
        
            var email = decodedToken.Claims["email"].ToString();
        
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        
            if (user == null)
                return Unauthorized("User not registered");
        
            var jwt = GenerateJwtToken(user);
        
            return Ok(new { token = jwt });
        }



        //PROFILE API (BACKEND)
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var user = await _context.Users
        .Where(u => u.Id == int.Parse(userId))
        .Select(u => new
        {
            u.Id,
            u.Name,
            u.Email,
            u.Role,

            u.PhoneNumber,
            u.Profession,
            u.BirthDate,

            u.ProfileImageUrl,
            u.BannerImageUrl,
            u.AboutMe,

            u.CreatedAt
        })
        .FirstOrDefaultAsync();


            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // ---------------- UPDATE PROFILE ----------------
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
                return NotFound("User not found");

            user.PhoneNumber = request.PhoneNumber;
            user.Profession = request.Profession;
            user.BirthDate = request.BirthDate;
            user.AboutMe = request.AboutMe;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }

}
