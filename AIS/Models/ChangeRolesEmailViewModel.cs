using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AIS.Models
{
    public class ChangeRolesEmailViewModel
    {
        // Properties with standard get and set accessors
        [Display(Name = "Admin")]
        public bool IsAdmin { get; set; }

        [Display(Name = "Client")]
        public bool IsClient { get; set; }

        [Display(Name = "Employee")]
        public bool IsEmployee { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public UserWithRolesViewModel UserWithRoles { get; set; }

        // Initialize the role properties in the GET action method
        public void GetRoles()
        {
            if (UserWithRoles != null)
            {
                IsAdmin = UserWithRoles.Roles.Contains("Admin");
                IsClient = UserWithRoles.Roles.Contains("Client");
                IsEmployee = UserWithRoles.Roles.Contains("Employee");
            }
        }
    }
}
