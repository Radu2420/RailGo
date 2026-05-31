using RailGo.Models;

namespace RailGo.ViewModels
{
    public class BookingConfirmationViewModel
    {
        public Booking Booking { get; set; } = new Booking();

        public Dictionary<int, string> TicketQrCodes { get; set; } = new Dictionary<int, string>();

        public string EmailMessage { get; set; } = string.Empty;
    }
}