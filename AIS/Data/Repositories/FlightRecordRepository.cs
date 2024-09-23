using AIS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AIS.Data.Repositories
{
    public class FlightRecordRepository : GenericRepository<TicketFlightRecord>, IFlightRecordRepository
    {
        public FlightRecordRepository(DataContext context) : base(context)
        {
        }
    }
}
