using AIS.Data.Entities;
using AIS.Models;
using System;
using System.Threading.Tasks;

namespace AIS.Helpers
{
    public interface IConverterHelper
    {
        Aircraft ToAircraft(AircraftViewModel model, string path, bool isNew);

        AircraftViewModel ToAircraftViewModel(Aircraft aircraft);

        Airport ToAirport(string iata, string country, string city);

        Flight ToFlight(Aircraft aircraft, Airport origin, Airport destination, FlightViewModel model, bool isNew);

        FlightViewModel ToFlightViewModel(Flight flight);
    }
}
