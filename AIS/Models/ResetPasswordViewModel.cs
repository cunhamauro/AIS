using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class ResetPasswordViewModel
    {
        public string UserId { get; set; }
        public string Token { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The Password and Confirmation Password do not match.")]
        [Display(Name = "Confirm Password")]
        public string Confirm { get; set; }
    }
}
