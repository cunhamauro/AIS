using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Data
{
    public class AirportRepository : GenericRepository<Airport>, IAirportRepository
    {
        private readonly DataContext _context;

        public AirportRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Check if a IATA code is repeated (Airport already created)
        /// </summary>
        /// <param name="iata">IATA Code</param>
        /// <returns>IATA already exists?</returns>
        public async Task<bool> IataExistsAsync(string iata)
        {
            return await _context.Airports.AsNoTracking().AnyAsync(a => a.IATA == iata);
        }

        /// <summary>
        /// Populate the Airport selection list
        /// </summary>
        /// <returns>Airport selection list</returns>
        public async Task<List<SelectListItem>> AirportSelectionList()
        {
            // Clone lists without reference
            List<Airport> listAirports = await _context.Airports.AsNoTracking().ToListAsync();

            // Make select item lists
            List<SelectListItem> selectAirportList = new List<SelectListItem>();

            // Fill the select item lists with items retrieved from DB
            foreach (Airport airport in listAirports)
            {
                selectAirportList.Add(new SelectListItem
                {
                    Value = airport.Id.ToString(),
                    Text = $"{airport.IATA} - {airport.City}, {airport.Country}",
                });
            }

            return selectAirportList;
        }
    }
}
