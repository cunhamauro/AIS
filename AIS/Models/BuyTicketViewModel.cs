using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AIS.Models
{
    public class BuyTicketViewModel : Ticket
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

        public decimal TicketPrice {  get; set; }

        public int FlightId { get; set; }

        public string FlightNumber { get; set; }

        public string OriginCityCountry {  get; set; }

        public string DestinationCityCountry { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime ArrivalDate { get; set; }
    }
}
