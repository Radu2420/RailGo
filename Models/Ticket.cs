using System.ComponentModel.DataAnnotations;

namespace RailGo.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int TripId { get; set; }
        public Trip? Trip { get; set; }

        [Required]
        public string TicketCode { get; set; } = string.Empty;

        public string DirectionType { get; set; } = "Dus";

        public int CarriageNumber { get; set; }

        public int SeatNumber { get; set; }

        public string SeatClass { get; set; } = "Clasa a II-a";

        public decimal Price { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.Now;

        public string QrCodeText { get; set; } = string.Empty;
    }
}