using System.ComponentModel.DataAnnotations;

namespace RailGo.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required]
        [Display(Name = "Utilizator")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Parolă")]
        public string Password { get; set; } = string.Empty;
    }
}