using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SpaceTravel.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SpaceTravel.Controllers
{
    // This controller handles authentication-related API requests.
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Endpoint for user login, which generates a JWT token upon successful authentication.
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest userLoginRequest)
        {
            // Authenticate user based on the provided username and password.
            var user = AuthenticateUser(userLoginRequest.Username, userLoginRequest.Password);

            if (user != null)
            {
                // Generate a JWT token.
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    token = token,
                    expiration = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiresInHours"]))
                });
            }

            return Unauthorized("Invalid username or password.");
        }

        // Helper method to authenticate a user based on username and password.
        private User AuthenticateUser(string username, string password)
        {
            var users = new[]
            {
                new User { Username = "user1", Password = "password1", Role = "cadet" },
                new User { Username = "user2", Password = "password2", Role = "captain" }
            };

            return Array.Find(users, u => u.Username == username && u.Password == password);
        }

        // Helper method to generate a JWT token for an authenticated user.
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiresInHours"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
