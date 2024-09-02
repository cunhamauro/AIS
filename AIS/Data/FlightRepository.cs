using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Data
{
    public class FlightRepository : GenericRepository<Flight>, IFlightRepository
    {
        private readonly DataContext _context;

        public FlightRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Flight> GetFlightIncludeByIdAsync(int id)
        {
            return await _context.Flights
                //.Include(f => f.User) // No user data needed (Even for the API Get it is unnecessary)
                .Include(f => f.Aircraft)
                .Include(f => f.Origin)
                .Include(f => f.Destination)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<Flight>> GetFlightsIncludeAsync()
        {
            return await _context.Flights
            .Include(f => f.Aircraft)
            .Include(f => f.Origin)
            .Include(f => f.Destination).ToListAsync();
        }

        public async Task<bool> AirportInFlights(int id)
        {
            return await _context.Flights.AnyAsync(a => a.Origin.Id == id || a.Destination.Id == id);
        }

        public async Task<bool> AircraftInFlights(int id)
        {
            return await _context.Flights.AnyAsync(a => a.Aircraft.Id == id);
        }
    }
}
