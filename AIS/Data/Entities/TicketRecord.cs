namespace AIS.Data.Entities
{
    public class TicketRecord : TicketFlightRecord, IEntity
    {
        public string UserEmail { get; set; }

        public decimal TicketPrice {  get; set; }

        public string Seat {  get; set; }
    }
}
