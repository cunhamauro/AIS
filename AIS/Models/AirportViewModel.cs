using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class AirportViewModel 
    {
        #region Properties

        [Display(Name = "Airport")]
        public string IATA {  get; set; }

        public IEnumerable<SelectListItem> IataList { get; set; }

        #endregion
    }
}
