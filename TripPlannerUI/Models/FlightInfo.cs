using System.ComponentModel.DataAnnotations;

namespace TripPlannerUI.Models
{
    public class FlightInfo
    {
        [Required]
        public double Price { get; set; }

        [Required]
        public Companies Company { get; set; }

        [Required]
        public Destinations From { get; set; }

        [Required]
        public Destinations To { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
