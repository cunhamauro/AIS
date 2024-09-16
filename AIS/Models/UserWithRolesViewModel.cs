using AIS.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class UserWithRolesViewModel
    {
        public User User { get; set; }

        [Display(Name = "Role")]
        public IList<string> Roles { get; set; }
    }
}
