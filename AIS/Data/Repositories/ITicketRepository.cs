using AIS.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIS.Data.Repositories
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<List<Ticket>> GetTicketsByUser(string id);
    }
}
