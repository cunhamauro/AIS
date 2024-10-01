using AIS.Data.Entities;

namespace AIS.Data.Repositories
{
    public class TicketRecordRepository : GenericRepository<TicketRecord>, ITicketRecordRepository
    {
        public TicketRecordRepository(DataContext context) : base(context)
        {
        }
    }
}
