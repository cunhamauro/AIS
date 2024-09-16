using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class TicketViewModel : Ticket
    {
        public IEnumerable<SelectListItem> SeatsList { get; set; }

        public IEnumerable<SelectListItem> TitlesList = new List<SelectListItem>
        {
            new SelectListItem { Value = "Mr.", Text = "Mr." },
            new SelectListItem { Value = "Mrs.", Text = "Mrs." },
            new SelectListItem { Value = "Miss", Text = "Miss" },
            new SelectListItem { Value = "Dr.", Text = "Dr." },
            new SelectListItem { Value = "Prof.", Text = "Prof." }
        };

        [Display(Name = "Flight ID")]
        public int FlightId { get; set; }

        [Display(Name = "Flight Number")]
        public string FlightNumber { get; set; }

        [Display(Name = "Origin")]
        public string OriginCityCountry {  get; set; }

        [Display(Name = "Destination")]
        public string DestinationCityCountry { get; set; }
 
        [Display(Name = "Departure")]
        public DateTime DepartureDate { get; set; }

        [Display(Name = "Arrival")]
        public DateTime ArrivalDate { get; set; }
    }
}
