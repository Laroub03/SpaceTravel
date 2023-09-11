using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using SpaceTravel.Models;

namespace SpaceTravel.Controllers
{
    // This controller handles API requests related to users.
    [Route("api/users")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        // Define constants and static variables for limiting API key requests.
        private const int MaxCadetRequestsPerHalfHour = 5;
        private static readonly Dictionary<string, int> CadetRequestCount = new Dictionary<string, int>();
        private static readonly object LockObject = new object();

        // Endpoint for generating an API key for cadets or returning a captain's API key.
        [HttpPost("getapikey")]
        public IActionResult GetApiKey([FromBody] User user)
        {
            if (user.Role == "cadet")
            {
                if (!CanGenerateCadetApiKey(user.Username))
                {
                    return Unauthorized("Maximum cadet API keys reached for the half-hour period.");
                }

                // Generate and return a new cadet API key.
                string newCadetApiKey = GenerateCadetApiKey();
                return Ok(new { apiKey = newCadetApiKey });
            }
            else if (user.Role == "captain")
            {
                // Check if the user is a valid captain.
                if (!CaptainApiKeys.Contains(user.Username))
                {
                    return Unauthorized("Invalid captain username.");
                }

                // Return the captain's API key.
                return Ok(new { apiKey = user.Username });
            }
            else
            {
                return BadRequest("Invalid user role.");
            }
        }

        // Helper method to check if a cadet can generate a new API key.
        private bool CanGenerateCadetApiKey(string username)
        {
            lock (LockObject)
            {
                if (CadetRequestCount.ContainsKey(username) && CadetRequestCount[username] >= MaxCadetRequestsPerHalfHour)
                {
                    return false;
                }
                return true;
            }
        }

        // Helper method to generate a new cadet API key.
        private string GenerateCadetApiKey()
        {
            lock (LockObject)
            {
                string newApiKey = $"cadetApiKey_{Guid.NewGuid()}";
                CadetRequestCount[newApiKey] = 0;
                return newApiKey;
            }
        }

        // Define a list of valid captain API keys.
        private static List<string> CaptainApiKeys = new List<string>
        {
            "captainApiKey1",
            "captainApiKey2"
        };
    }
}
