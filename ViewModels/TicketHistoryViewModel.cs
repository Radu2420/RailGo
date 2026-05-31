using System.ComponentModel.DataAnnotations;
using RailGo.Models;

namespace RailGo.ViewModels
{
    public class TicketHistoryViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email pasager")]
        public string PassengerEmail { get; set; } = string.Empty;

        public bool HasSearched { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}