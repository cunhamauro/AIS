using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AIS.Models
{
    public class EditUserViewModel
    {
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "User ID")]
        public string UserId {  get; set; }
    }
}
