using System.ComponentModel.DataAnnotations;

namespace RailGo.Models
{
    public class RailRoute
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Stație plecare")]
        public int DepartureStationId { get; set; }

        [Display(Name = "Stație plecare")]
        public Station? DepartureStation { get; set; }

        [Required]
        [Display(Name = "Stație sosire")]
        public int ArrivalStationId { get; set; }

        [Display(Name = "Stație sosire")]
        public Station? ArrivalStation { get; set; }

        [Display(Name = "Distanță km")]
        public int DistanceKm { get; set; }

        [Display(Name = "Durată minute")]
        public int DurationMinutes { get; set; }
    }
}