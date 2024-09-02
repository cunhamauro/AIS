using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Data
{
    public interface IAirportRepository : IGenericRepository<Airport>
    {
        Task<bool> IataExistsAsync(string iata);

        Task<List<SelectListItem>> AirportSelectionList();
    }
}
