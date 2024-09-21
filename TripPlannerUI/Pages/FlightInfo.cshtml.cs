using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TripPlannerUI.Models;
using System.Collections.Generic;
using System.Linq;

namespace TripPlannerUI.Pages
{
    public class FlightInfoModel : PageModel
    {
        [BindProperty]
        public FlightInfo? Flight { get; set; }

        public List<FlightInfo> Flights { get; set; } = new List<FlightInfo>();

        public List<Route>? PossibleRoutes { get; set; }

        public void OnGet()
        {
            if (TempData.ContainsKey("Flights"))
            {
                Flights = TempData.Get<List<FlightInfo>>("Flights");
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage); // or use a logger
                    }
                }
                return Page();  // Return the page to show validation errors
            }


            // Load existing flights from TempData
            if (TempData.ContainsKey("Flights"))
            {
                Flights = TempData.Get<List<FlightInfo>>("Flights");
            }

            // Add the new flight
            Flights.Add(Flight);

            // Save the list back to TempData
            TempData.Put("Flights", Flights);

            return RedirectToPage();
        }

        public IActionResult OnPostCalculateRoutes()
        {
            if (TempData.ContainsKey("Flights"))
            {
                Flights = TempData.Get<List<FlightInfo>>("Flights");
            }

            // Calculate all possible routes
            PossibleRoutes = CalculateRoutes(Flights);

            // Save the routes so they can be displayed
            return Page();
        }

        private List<Route> CalculateRoutes(List<FlightInfo> flights)
        {
            var routes = new List<Route>();

            foreach (var flight in flights)
            {
                var route = new Route
                {
                    Stops = new List<string> { flight.From.ToString(), flight.To.ToString() },
                    TotalPrice = flight.Price
                };
                routes.Add(route);
            }

            return routes.OrderBy(r => r.TotalPrice).ToList();
        }
    }

    public class Route
    {
        public List<string> Stops { get; set; }
        public double TotalPrice { get; set; }
    }
}
