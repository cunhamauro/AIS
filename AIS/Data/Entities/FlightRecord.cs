using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System;

namespace AIS.Data.Entities
{
    public class FlightRecord : IEntity
    {
        #region Properties

        public int Id { get; set; }

        public string FlightNumber { get; set; }

        public int AircraftId {  get; set; }

        public int OriginId { get; set; }

        public int DestinationId{ get; set; }

        public DateTime Departure { get; set; }

        public DateTime Arrival { get; set; }

        public int PassengerCount { get; set; }

        #endregion
    }
}
