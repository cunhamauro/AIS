using AIS.Data.Entities;
using System.ComponentModel;

namespace AIS.Models
{
    public class DashboardViewModel
    {
        [DisplayName("Most Popular Destination")]
        public Airport MostPopularDestination { get; set; }

        [DisplayName("Airports")]
        public int AirportsCount { get; set; }

        [DisplayName("Aircrafts")]
        public int AircraftsCount { get; set; }

        [DisplayName("Clients")]
        public int ClientsCount { get; set; }

        [DisplayName("Employees")]
        public int EmployeesCount { get; set; }

        [DisplayName("Admins")]
        public int AdminsCount { get; set; }

        [DisplayName("Active Flights")]
        public int ActiveFlightsCount { get; set; }

        [DisplayName("Canceled Flights")]
        public int CanceledFlightsCount { get; set; }

        [DisplayName("Total Flights")]
        public int FlightRecordsCount {  get; set; }

        [DisplayName("Active Tickets")]
        public int ActiveTicketsCount { get; set; }

        [DisplayName("Total Tickets")]
        public int TicketRecordsCount {  get; set; }

        [DisplayName("Total Revenue")]
        public decimal MoneyTotalTickets { get; set; }
    }
}
