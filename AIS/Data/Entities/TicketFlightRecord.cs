using System;
using System.ComponentModel.DataAnnotations;

namespace AIS.Data.Entities
{
    public class TicketFlightRecord : IEntity
    {
        #region Properties

        public int Id { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Holder ID Number")]
        public string HolderIdNumber {  get; set; }

        public string Seat {  get; set; }

        [Display(Name = "Ticket Price")]
        public decimal TicketPrice {  get; set; }

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

        #endregion
    }
}
