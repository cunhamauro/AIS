using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class FlightViewModel : Flight
    {
        [Required]
        [Display(Name = "Aircraft")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Flight Aircraft!")]
        public int AircraftId { get; set; }

        [Required]
        [Display(Name = "Origin")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Flight Origin!")]
        public int OriginId { get; set; }

        [Required]
        [Display(Name = "Destination")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Flight Destination!")]
        public int DestinationId { get; set; }

        public IEnumerable<SelectListItem> OriginList { get; set; }

        public IEnumerable<SelectListItem> DestinationList { get; set; }

        public IEnumerable<SelectListItem> AircraftList { get; set; }
    }
}
