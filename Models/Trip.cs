using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailGo.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Rută")]
        public int RailRouteId { get; set; }

        [Display(Name = "Rută")]
        public RailRoute? RailRoute { get; set; }

        [Required]
        [Display(Name = "Tren")]
        public int TrainId { get; set; }

        [Display(Name = "Tren")]
        public Train? Train { get; set; }

        [Display(Name = "Data și ora plecării")]
        public DateTime DepartureTime { get; set; }

        [Display(Name = "Data și ora sosirii")]
        public DateTime ArrivalTime { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Preț")]
        public decimal Price { get; set; }

        [Display(Name = "Activă")]
        public bool IsActive { get; set; } = true;
    }
}