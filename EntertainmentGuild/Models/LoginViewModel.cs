using System.ComponentModel.DataAnnotations;

namespace EntertainmentGuild.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Role { get; set; }

        public bool RememberMe { get; set; }
    }
}
