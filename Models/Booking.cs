using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailGo.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Cod rezervare")]
        public string BookingCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nume pasager")]
        public string PassengerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string PassengerEmail { get; set; } = string.Empty;

        [Display(Name = "Tip pasager")]
        public string PassengerType { get; set; } = "Adult";

        [Display(Name = "Este student")]
        public bool IsStudent { get; set; }

        [Display(Name = "Universitate")]
        public string University { get; set; } = string.Empty;

        [Display(Name = "Număr legitimație student")]
        public string StudentCardNumber { get; set; } = string.Empty;

        [Display(Name = "Tip bilet")]
        public string TicketType { get; set; } = "Dus";

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Preț total")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Data rezervării")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}