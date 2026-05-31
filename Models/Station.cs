using System.ComponentModel.DataAnnotations;

namespace RailGo.Models
{
    public class Station
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nume stație")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Oraș")]
        public string City { get; set; } = string.Empty;
    }
}