using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
