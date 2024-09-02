using AIS.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AIS.Services
{
    public interface IAircraftAvailabilityService
    {
        Task<List<Aircraft>> AvailableAircrafts(DateTime departure, DateTime arrival, Airport origin);

        Task<bool> AircraftAvailableOnDate(Aircraft aircraft, DateTime checkDateDeparture, DateTime checkDateArrival, Airport origin);
    }
}
