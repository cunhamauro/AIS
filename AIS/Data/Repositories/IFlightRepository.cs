using AIS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Data.Repositories
{
    public interface IFlightRepository : IGenericRepository<Flight>
    {
        Task DeleteOldFlightsAsync();

        Task<Flight> GetFlightTrackIncludeByIdAsync(int id);

        Task<List<Flight>> GetFlightsTrackIncludeAsync();

        Task<bool> AirportInFlights(int id);

        Task<bool> AircraftInFlights(int id);

        Task FlightsFromUserToAdmin(List<Flight> userFlights, User admin);
    }
}
