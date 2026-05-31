using System.ComponentModel.DataAnnotations;

namespace RailGo.Models
{
    public class Train
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nume tren")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cod tren")]
        public string TrainCode { get; set; } = string.Empty;

        [Display(Name = "Număr vagoane")]
        public int CarriageCount { get; set; }

        [Display(Name = "Locuri pe vagon")]
        public int SeatsPerCarriage { get; set; }
    }
}