namespace TripPlanner.Models
{
    public class FlightInfo
    {
        public double Price { get; set; }
        public Companies Company { get; set; }
        public Destinations From { get; set; }
        public Destinations To { get; set; }
        public DateTime Date { get; set; }
    }
}
