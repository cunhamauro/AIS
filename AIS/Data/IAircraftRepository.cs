using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Data
{
    public interface IAircraftRepository : IGenericRepository<Aircraft>
    {
        Task<List<SelectListItem>> AircraftSelectionList();
    }
}
