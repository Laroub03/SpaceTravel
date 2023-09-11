using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using SpaceTravel.Models;

namespace SpaceTravel.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace SpaceTravel.Controllers
    {
        [Route("api/routes")]
        [ApiController]
        public class GalacticRoutesController : ControllerBase
        {
            private static List<GalacticRoute> routes = new List<GalacticRoute>
            {
                
            };

            // GET api/routes
            [HttpGet]
            public IActionResult GetAllRoutes()
            {

                return Ok(routes);
            }

            // GET api/routes/{routeName}
            [HttpGet("{routeName}")]
            public IActionResult GetRouteByName(string routeName)
            {
              
                var route = routes.FirstOrDefault(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
                if (route == null)
                {
                    return NotFound();
                }
                return Ok(route);
            }

            // POST api/routes
            [HttpPost]
            public IActionResult CreateRoute([FromBody] GalacticRoute newRoute)
            {
                // Implements logic that adds a new route
               
                routes.Add(newRoute);
                return CreatedAtAction("GetRouteByName", new { routeName = newRoute.Name }, newRoute);
            }

            // PUT api/routes/{routeName}
            [HttpPut("{routeName}")]
            public IActionResult UpdateRoute(string routeName, [FromBody] GalacticRoute updatedRoute)
            {

                // Implements logic that updates a existing route
                var existingRoute = routes.FirstOrDefault(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
                if (existingRoute == null)
                {
                    return NotFound();
                }

                // Updates route infomation
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
            public IActionResult DeleteRoute(string routeName)
            {
                // Implements logic that deletes an existing route
                var routeToRemove = routes.FirstOrDefault(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
                if (routeToRemove == null)
                {
                    return NotFound();
                }

                routes.Remove(routeToRemove);
                return NoContent();
            }

        }
    }
}
