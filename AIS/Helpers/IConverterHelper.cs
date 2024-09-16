using AIS.Data.Entities;
using AIS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Helpers
{
    public interface IConverterHelper
    {
        User ToUser(CreateUserViewModel model);
        Aircraft ToAircraft(AircraftViewModel model, string path, bool isNew);

        AircraftViewModel ToAircraftViewModel(Aircraft aircraft);

        Airport ToAirport(string iata, string country, string city);

        Flight ToFlight(Aircraft aircraft, Airport origin, Airport destination, FlightViewModel model, bool isNew);

        FlightViewModel ToFlightViewModel(Flight flight);

        TicketViewModel ToTicketViewModel(Ticket ticket, Flight flight, List<SelectListItem> listSeats);
    }
}
