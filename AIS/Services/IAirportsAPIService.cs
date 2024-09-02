using AIS.Data;
using AIS.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Services
{
    public interface IAirportsAPIService
    {
        Task<List<Airport>> GetAirportsAsync();
    }
}
