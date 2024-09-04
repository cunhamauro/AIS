using AIS.Data.Entities;
using System.Collections.Generic;

namespace AIS.Models
{
    public class UserWithRolesViewModel
    {
        public User User { get; set; }
        public IList<string> Roles { get; set; }
    }
}
