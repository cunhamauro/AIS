using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class FlightsFiltersViewModel
    {
        public List<Flight> Flights {  get; set; }

        [Display(Name = "Origin")]
        public int OriginId { get; set; }

        [Display(Name = "Filter by Origin")]
        public bool FilterByOrigin { get; set; }

        [Display(Name = "Destination")]
        public int DestinationId { get; set; }

        [Display(Name = "Filter by Destination")]
        public bool FilterByDestination { get; set; }

        public DateTime Departure { get; set; }

        [Display(Name = "Filter by Departure")]
        public bool FilterByDeparture { get; set; }

        public DateTime Arrival { get; set; }

        [Display(Name = "Filter by Arrival")]
        public bool FilterByArrival { get; set; }

        public IEnumerable<SelectListItem> OriginList { get; set; }

        public IEnumerable<SelectListItem> DestinationList { get; set; }
    }
}
