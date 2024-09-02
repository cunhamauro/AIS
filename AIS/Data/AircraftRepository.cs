using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Data
{
    public class AircraftRepository : GenericRepository<Aircraft>, IAircraftRepository
    {
        private readonly DataContext _context;

        public AircraftRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Populate the Aircraft selection list
        /// </summary>
        /// <returns>Aircraft selection list</returns>
        public async Task<List<SelectListItem>> AircraftSelectionList()
        {
            // Clone lists without reference
            List<Aircraft> listAircrafts = await _context.Aircrafts.AsNoTracking().ToListAsync();

            // Make select item lists
            List<SelectListItem> selectAircraftList = new List<SelectListItem>();

            // Fill the select item lists with items retrieved from DB
            foreach (Aircraft aircraft in listAircrafts)
            {
                selectAircraftList.Add(new SelectListItem
                {
                    Value = aircraft.Id.ToString(),
                    Text = $"{aircraft.Id} - {aircraft.Model} (Capacity: {aircraft.Capacity})",
                });
            }

            return selectAircraftList;
        }
    }
}
