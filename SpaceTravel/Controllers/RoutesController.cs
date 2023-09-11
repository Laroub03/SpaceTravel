using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpaceTravel.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SpaceTravel.Controllers
{
    // This controller handles API requests related to galactic routes.
    [Authorize]
    [Route("api/routes")]
    [ApiController]
    public class GalacticRoutesController : ControllerBase
    {
        private static List<GalacticRoute> routes;
        private const int MaxCadetRequestsPerHalfHour = 5;
        private static readonly Dictionary<string, int> CadetRequestCount = new Dictionary<string, int>();
        private static readonly object LockObject = new object();
        private static DateTime HalfHourResetTime = DateTime.MinValue;
        private static List<string> CaptainApiKeys = new List<string>();
        private static List<string> CadetApiKeys = new List<string>();
        private static List<string> AuthorizedApiKeys = new List<string>();
        private static List<string> ValidApiKeys = new List<string>();
        private static List<string> UnauthorizedApiKeys = new List<string>();

        static GalacticRoutesController()
        {
            try
            {
                // Load galactic routes from a JSON file.
                routes = LoadRoutesFromJson();
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., log the error or take appropriate action.
                Console.WriteLine($"Error loading routes from JSON: {ex.Message}");
                routes = new List<GalacticRoute>();
            }
        }

        // GET api/routes
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Requires authentication
        public IActionResult GetAllRoutes()
        {
            // User is authenticated, proceed to retrieve routes.
            return Ok(routes);
        }

        // GET api/routes/{routeName}
        [HttpGet("{routeName}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Requires authentication
        public IActionResult GetRouteByName(string routeName)
        {
            // User is authenticated, proceed to retrieve the route.
            var route = routes.FirstOrDefault(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
            if (route == null)
            {
                return NotFound();
            }

            return Ok(route);
        }

        // POST api/routes
        [HttpPost]
        [Authorize(Roles = "captain", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Requires authentication and captain role
        public IActionResult CreateRoute([FromBody] GalacticRoute newRoute)
        {
            // Only captains can create routes.
            routes.Add(newRoute);

            return CreatedAtAction("GetRouteByName", new { routeName = newRoute.Name }, newRoute);
        }

        // PUT api/routes/{routeName}
        [HttpPut("{routeName}")]
        [Authorize(Roles = "captain", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Requires authentication and captain role
        public IActionResult UpdateRoute(string routeName, [FromBody] GalacticRoute updatedRoute)
        {
            // Only captains can update routes.
            var existingRoute = routes.FirstOrDefault(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
            if (existingRoute == null)
            {
                return NotFound();
            }

            // Update route information.
            existingRoute.Name = updatedRoute.Name;
            existingRoute.Start = updatedRoute.Start;
            existingRoute.End = updatedRoute.End;
            existingRoute.NavigationPoints = updatedRoute.NavigationPoints;
            existingRoute.Duration = updatedRoute.Duration;
            existingRoute.Dangers = updatedRoute.Dangers;
            existingRoute.FuelUsage = updatedRoute.FuelUsage;
            existingRoute.Description = updatedRoute.Description;

            return NoContent();
        }

        // DELETE api/routes/{routeName}
        [HttpDelete("{routeName}")]
        [Authorize(Roles = "captain", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Requires authentication and captain role
        public IActionResult DeleteRoute(string routeName)
        {
            // Only captains can delete routes.
            var routeToRemove = routes.FirstOrDefault(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
            if (routeToRemove == null)
            {
                return NotFound();
            }

            routes.Remove(routeToRemove);

            return NoContent();
        }

        // Helper method to check API key authorization and rate limiting.
        private bool IsAuthorized(string apiKey)
        {
            lock (LockObject)
            {
                if (UnauthorizedApiKeys.Contains(apiKey))
                {
                    return false; // Invalid API key, previously denied access
                }

                if (!ValidApiKeys.Contains(apiKey))
                {
                    UnauthorizedApiKeys.Add(apiKey);
                    return false; // Invalid API key, recorded as denied access
                }

                // Extract user role from the API key (assuming it contains role information)
                string userRole = GetUserRole(apiKey);

                if (string.IsNullOrEmpty(userRole))
                {
                    return false; // Invalid API key, role information missing or incorrect
                }

                // Use a switch-case to handle different roles/access levels
                switch (userRole)
                {
                    case "cadet":
                        if (DateTime.Now > HalfHourResetTime)
                        {
                            CadetRequestCount.Clear(); // Reset cadet requests after half an hour
                            HalfHourResetTime = DateTime.Now.AddMinutes(30);
                        }

                        if (!CadetRequestCount.ContainsKey(apiKey))
                        {
                            CadetRequestCount[apiKey] = 0;
                        }

                        if (CadetRequestCount[apiKey] >= MaxCadetRequestsPerHalfHour)
                        {
                            UnauthorizedApiKeys.Add(apiKey); // Deny further access when cadet reaches request limit
                            return false;
                        }

                        CadetRequestCount[apiKey]++;
                        break;

                    case "captain":
                        // Captains have unlimited access, no restrictions
                        break;

                    default:
                        return false; // Invalid API key, unrecognized role
                }

                AuthorizedApiKeys.Add(apiKey);
                return true;
            }
        }

        // Helper method to extract the user role from an API key.
        private string GetUserRole(string apiKey)
        {
            // Example:
            if (CadetApiKeys.Contains(apiKey))
            {
                return "cadet";
            }
            else if (CaptainApiKeys.Contains(apiKey))
            {
                return "captain";
            }

            return null; // Invalid or unrecognized API key
        }

        // Helper method to load galactic routes from a JSON file.
        private static List<GalacticRoute> LoadRoutesFromJson()
        {
            try
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "galacticRoutes.json");

                if (!System.IO.File.Exists(filePath))
                {
                    // Handle the case where the JSON file does not exist.
                    Console.WriteLine("JSON file not found.");
                    return new List<GalacticRoute>();
                }

                var jsonData = System.IO.File.ReadAllText(filePath);
                var wrapper = JsonSerializer.Deserialize<GalacticRoutesWrapper>(jsonData);

                if (wrapper != null && wrapper.GalacticRoutes != null)
                {
                    return wrapper.GalacticRoutes;
                }
                else
                {
                    // Handle the case where the JSON data is missing or invalid.
                    Console.WriteLine("Invalid JSON data format.");
                    return new List<GalacticRoute>();
                }
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., log the error or take appropriate action.
                Console.WriteLine($"Error loading routes from JSON: {ex.Message}");
                Console.WriteLine(ex.StackTrace); // Log the stack trace for more details
                return new List<GalacticRoute>();
            }
        }
    }
}
