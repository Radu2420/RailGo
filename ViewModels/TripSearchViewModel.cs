using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using RailGo.Models;

namespace RailGo.ViewModels
{
    public class TripSearchViewModel
    {
        [Required(ErrorMessage = "Alege stația de plecare.")]
        [Display(Name = "Stație plecare")]
        public int? DepartureStationId { get; set; }

        [Required(ErrorMessage = "Alege stația de sosire.")]
        [Display(Name = "Stație sosire")]
        public int? ArrivalStationId { get; set; }

        [Required(ErrorMessage = "Alege data călătoriei.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data călătoriei")]
        public DateTime TravelDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Data întoarcerii")]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Tip bilet")]
        public string TicketType { get; set; } = "Dus";

        [Display(Name = "Clasă")]
        public string SelectedClass { get; set; } = "Clasa a II-a";

        public bool HasSearched { get; set; }

        public List<SelectListItem> Stations { get; set; } = new();

        public List<SelectListItem> ArrivalStations { get; set; } = new();

        // Folosit de noua pagină Search.cshtml
        public List<Trip> Trips { get; set; } = new();

        // Folosit de TripsController.cs-ul tău actual
        public List<Trip> Results
        {
            get => Trips;
            set => Trips = value ?? new List<Trip>();
        }

        // Pentru bilet dus-întors
        public List<Trip> ReturnTrips { get; set; } = new();

        // Folosit de TripsController.cs-ul tău actual pentru dus-întors
        public List<RoundTripSearchResultViewModel> RoundTripResults { get; set; } = new();
    }

    public class RoundTripSearchResultViewModel
    {
        public Trip? OutboundTrip { get; set; }

        public Trip? ReturnTrip { get; set; }

        public decimal OutboundPrice { get; set; }

        public decimal ReturnPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public string RouteName
        {
            get
            {
                var departure = OutboundTrip?.RailRoute?.DepartureStation?.Name ?? "";
                var arrival = OutboundTrip?.RailRoute?.ArrivalStation?.Name ?? "";

                if (string.IsNullOrWhiteSpace(departure) || string.IsNullOrWhiteSpace(arrival))
                {
                    return "";
                }

                return $"{departure} → {arrival}";
            }
        }
    }
}