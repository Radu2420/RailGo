using System.ComponentModel.DataAnnotations;
using RailGo.Models;

namespace RailGo.ViewModels
{
    public class SeatSelectionViewModel
    {
        public int TripId { get; set; }

        public Trip? Trip { get; set; }

        public int? ReturnTripId { get; set; }

        public Trip? ReturnTrip { get; set; }

        public string TicketType { get; set; } = "Dus";

        public bool IsRoundTrip => TicketType == "Dus-întors" && ReturnTripId.HasValue;

        [Required(ErrorMessage = "Numele pasagerului este obligatoriu.")]
        [Display(Name = "Nume pasager")]
        public string PassengerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Emailul este obligatoriu.")]
        [EmailAddress(ErrorMessage = "Introdu o adresă de email validă.")]
        [Display(Name = "Email")]
        public string PassengerEmail { get; set; } = string.Empty;

        [Display(Name = "Tip pasager")]
        public string PassengerType { get; set; } = "Adult";

        [Display(Name = "Universitate")]
        public string University { get; set; } = string.Empty;

        [Display(Name = "Număr legitimație student")]
        public string StudentCardNumber { get; set; } = string.Empty;

        public int SelectedCarriage { get; set; } = 1;

        public int SelectedSeat { get; set; }

        public int? ReturnSelectedCarriage { get; set; }

        public int? ReturnSelectedSeat { get; set; }

        public List<int> OccupiedSeats { get; set; } = new List<int>();

        public List<int> ReturnOccupiedSeats { get; set; } = new List<int>();

        public decimal OutboundNormalPrice { get; set; }

        public decimal OutboundFinalPrice { get; set; }

        public decimal ReturnNormalPrice { get; set; }

        public decimal ReturnFinalPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public string SeatClass { get; set; } = "Clasa I";

        public string ReturnSeatClass { get; set; } = "Clasa I";

        public int CarriageCount { get; set; } = 1;

        public int SeatsPerCarriage { get; set; } = 40;

        public int ReturnCarriageCount { get; set; } = 1;

        public int ReturnSeatsPerCarriage { get; set; } = 40;
    }
}