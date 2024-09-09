using AIS.Data.Entities;

namespace AIS.Data.Repositories
{
    public class FlightRecordRepository : GenericRepository<FlightRecord>, IFlightRecordRepository
    {
        public FlightRecordRepository(DataContext context) : base(context)
        {
        }
    }
}
