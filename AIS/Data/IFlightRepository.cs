using AIS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Data
{
    public interface IFlightRepository : IGenericRepository<Flight>
    {
        Task<Flight> GetFlightIncludeByIdAsync(int id);

        Task<List<Flight>> GetFlightsIncludeAsync();

        Task<bool> AirportInFlights(int id);

        Task<bool> AircraftInFlights(int id);
    }
}
