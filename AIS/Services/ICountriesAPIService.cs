using AIS.Data.Classes;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Services
{
    public interface ICountriesAPIService
    {
        Task<List<Country>> GetCountriesAsync();
    }
}
