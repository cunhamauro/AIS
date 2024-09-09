using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class CreateUserViewModel : User
    {
        [Display(Name = "Role")]
        public IEnumerable<SelectListItem> RolesList { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Admin", Text = "Admin" },
            new SelectListItem { Value = "Client", Text = "Client" },
            new SelectListItem { Value = "Employee", Text = "Employee" }
        };

        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }
}
