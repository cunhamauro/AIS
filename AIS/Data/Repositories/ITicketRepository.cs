using AIS.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Data.Repositories
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<bool> ClientHasTickets(string userId);

        Task<Ticket> GetTicketIncludeFlightAirportsAsync(int id);

        Task<List<Ticket>> GetTicketsByUserIncludeFlightAirportsAsync(string id);
    }
}
