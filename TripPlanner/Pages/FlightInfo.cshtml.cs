using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TripPlanner.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TripPlanner.Pages
{
    public class FlightInfoModel : PageModel
    {
        [BindProperty]
        public FlightInfo? Flight { get; set; } = null;

        public List<FlightInfo> Flights { get; set; } = new List<FlightInfo>();

        public List<Route>? PossibleRoutes { get; set; }

        public void OnGet()
        {
            var flightsJson = HttpContext.Session.GetString("Flights");
            if (!string.IsNullOrEmpty(flightsJson))
            {
                Flights = JsonConvert.DeserializeObject<List<FlightInfo>>(flightsJson);
            }
            else
            {
                Flights = new List<FlightInfo>(); // Initialize with an empty list if no flights are found
            }

            var routesJson = HttpContext.Session.GetString("PossibleRoutes");
            if (!string.IsNullOrEmpty(routesJson))
            {
                PossibleRoutes = JsonConvert.DeserializeObject<List<Route>>(routesJson);
            }
        }


        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // Return the page to show validation errors
            }

            if (HttpContext.Session.GetString("Flights") != null)
            {
                Flights = JsonConvert.DeserializeObject<List<FlightInfo>>(HttpContext.Session.GetString("Flights"));
            }
            else
            {
                Flights = new List<FlightInfo>(); // Create a new list if none exists
            }

            // Add the new flight
            Flights?.Add(Flight);

            // Save the updated list back to Session
            HttpContext.Session.SetString("Flights", JsonConvert.SerializeObject(Flights));

            return RedirectToPage();
        }

        public IActionResult OnPostRemoveFlight(int index)
        {
            if (HttpContext.Session.GetString("Flights") != null)
            {
                var flights = JsonConvert.DeserializeObject<List<FlightInfo>>(HttpContext.Session.GetString("Flights"));
                if (index >= 0 && index < flights?.Count)
                {
                    flights.RemoveAt(index); // Remove the flight at the specified index
                }

                // Save the updated list back to Session
                HttpContext.Session.SetString("Flights", JsonConvert.SerializeObject(flights));
            }

            return RedirectToPage(); // Redirect back to the same page to refresh
        }
        public IActionResult OnPostCalculateRoutes()
        {
            var flightsJson = HttpContext.Session.GetString("Flights");
            if (string.IsNullOrEmpty(flightsJson))
            {
                // Handle case where there are no flights
                ModelState.AddModelError(string.Empty, "No flights available for route calculation.");
                return Page();
            }

            var flights = JsonConvert.DeserializeObject<List<FlightInfo>>(flightsJson);
            PossibleRoutes = GetAllRoutes(flights);

            HttpContext.Session.SetString("PossibleRoutes", JsonConvert.SerializeObject(PossibleRoutes));
            return RedirectToPage();
        }

        private List<Route> GetAllRoutes(List<FlightInfo> flights)
        {
            var routes = new List<Route>();
            var usedFlights = new bool[flights.Count];

            // Start building routes
            BuildRoutes(new List<FlightInfo>(), flights, usedFlights, routes);

            // Filter routes to include only those with at least 3 destinations
            return routes.Where(r => r.Stops.Count >= 2).ToList();
        }

        private void BuildRoutes(List<FlightInfo> currentRoute, List<FlightInfo> flights, bool[] usedFlights, List<Route> routes)
        {
            // If currentRoute is not empty and has at least 3 destinations, format the output string
            if (currentRoute.Count >= 2)
            {
                var routeDescription = FormatRoute(currentRoute);
                routes.Add(routeDescription);
            }

            for (int i = 0; i < flights.Count; i++)
            {
                if (!usedFlights[i])
                {
                    // Check if the current flight can be added to the route
                    if (currentRoute.Count == 0 ||
                        (currentRoute.Last().To == flights[i].From && currentRoute.Last().Date <= flights[i].Date))
                    {
                        // Mark flight as used
                        usedFlights[i] = true;
                        currentRoute.Add(flights[i]);

                        // Recur to build further routes
                        BuildRoutes(currentRoute, flights, usedFlights, routes);

                        // Backtrack
                        currentRoute.RemoveAt(currentRoute.Count - 1);
                        usedFlights[i] = false;
                    }
                }
            }
        }

        private Route FormatRoute(List<FlightInfo> route)
        {
            var stops = new List<string>();
            var totalPrice = 0.0;

            foreach (var flight in route)
            {
                stops.Add($"<strong>{flight.From}</strong> -[{flight.Date.ToShortDateString()} - {flight.Price} EUR]><strong>{flight.To}</strong>");
                totalPrice += flight.Price;
            }

            return new Route
            {
                Stops = stops,
                TotalPrice = totalPrice
            };
        }

    }

    public class Route
    {
        public List<string> Stops { get; set; }
        public double TotalPrice { get; set; }
    }
}
