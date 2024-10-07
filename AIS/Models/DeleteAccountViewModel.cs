using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class DeleteAccountViewModel
    {
        [Required]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The Password and Confirmation Password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
