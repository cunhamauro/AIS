using System.ComponentModel.DataAnnotations;
using System;

namespace AIS.Data.Entities
{
    public class FlightRecord : IEntity
    {
        public int Id { get; set; } // Flight Id

        [Display(Name = "Flight Number")]
        public string FlightNumber { get; set; }

        public string OriginCity { get; set; }

        public string OriginCountry { get; set; }

        public string OriginFlagImageUrl { get; set; }

        public string DestinationCity { get; set; }

        public string DestinationCountry { get; set; }

        public string DestinationFlagImageUrl { get; set; }

        public DateTime Departure { get; set; }

        public DateTime Arrival { get; set; }

        public bool Canceled { get; set; }
    }
}
