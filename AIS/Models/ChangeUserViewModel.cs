using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class ChangeUserViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile ImageFile { get; set; }

        [Display(Name = "Profile Image")]
        public string ImageUrl { get; set; }

        [Display(Name = "Profile Image")]
        public string ImageDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(ImageUrl))
                {
                    return $"/images/default-profile-image.png";
                }
                else
                {
                    return $"{ImageUrl.Substring(1)}";
                }
            }
        }
    }
}
