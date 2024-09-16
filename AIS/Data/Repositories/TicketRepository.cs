using AIS.Data.Entities;
using AIS.Helpers;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AIS.Data.Repositories
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public TicketRepository(DataContext context, IUserHelper userHelper) : base(context)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task<Ticket> GetTicketIncludeFlightAirportsAsync(int id)
        {
            return await _context.Tickets.Include(f => f.Flight).ThenInclude(a => a.Origin).Include(f => f.Flight).ThenInclude(a => a.Destination).FirstOrDefaultAsync(t => t.Id == id);
        } 

        public async Task<List<Ticket>> GetTicketsByUserIncludeFlightAirportsAsync(string id)
        {
            User user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                return new List<Ticket>();
            }

            return await _context.Tickets.Where(t => t.User.Id == id).Include(f => f.Flight).ThenInclude(a => a.Origin).Include(f => f.Flight).ThenInclude(a => a.Destination).ToListAsync();
        }
    }
}
