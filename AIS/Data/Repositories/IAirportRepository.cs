using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Data.Repositories
{
    public interface IAirportRepository : IGenericRepository<Airport>
    {
        Task<bool> IataExistsAsync(string iata);

        Task<List<SelectListItem>> AirportSelectionList();

        Task AirportsFromUserToAdmin(List<Airport> userAirports, User admin);
    }
}
