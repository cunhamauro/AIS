using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [Display(Name = "New Password")]
        [MinLength(6)]

        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The New Password and Confirmation Password do not match.")]
        [Display(Name = "Confirm New Password")]
        public string Confirm { get; set; }
    }
}
