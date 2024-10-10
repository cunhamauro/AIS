using AIS.Data.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Data.Repositories
{
    public class TicketRecordRepository : GenericRepository<TicketRecord>, ITicketRecordRepository
    {
        private readonly DataContext _context;

        public TicketRecordRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<TicketRecord>> GetAllNonCanceledTicketRecords()
        {
            List<TicketRecord> records = await _context.TicketRecords.ToListAsync();

            return records.Where(r => r.Canceled == false).ToList();
        }
    }
}
