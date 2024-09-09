using Syncfusion.EJ2.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace AIS.Data.Entities
{
    public class Flight : IEntity
    {
        #region Attributes

        private Aircraft _aircraft;

        #endregion

        #region Properties

        public int Id { get; set; }

        public Aircraft Aircraft
        {
            get
            {
                return _aircraft;
            }
            set
            {
                _aircraft = value;
                AvailableSeats = new List<string>(Aircraft.Seats); // Load the seats from the chosen aircraft to allocate them for the flight
            }
        }

        [Display(Name = "Available Seats")]
        public List<string> AvailableSeats { get; set; }

        public Airport Origin { get; set; }

        public Airport Destination { get; set; }

        [Required]
        public DateTime Departure {  get; set; }

        [Required]
        public DateTime Arrival { get; set; }

        public TimeSpan Duration => Arrival - Departure;

        [Display(Name = "Flight Number")]
        public string FlightNumber {  get; set; }

        public User User { get; set; }

        public List<Ticket> TicketList { get; set; } = new List<Ticket>();

        public int PassengerCount { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Generate FlightNumber [ID][Origin.IATA][Destination.IATA]
        /// </summary>
        /// <param name="id">Flight ID</param>
        public void GenerateFlightNumber(int id, string originIata, string destinationIata)
        {
            // Generated with parameter IATA strings to no bother with fetching the origin and destination Airport entity with include from the database
            FlightNumber = $"{id}{originIata}{destinationIata}";
        }

        /// <summary>
        /// Format the list of seats for display
        /// </summary>
        public string FormatAvailableSeats()
        {
            if (AvailableSeats == null || !AvailableSeats.Any())
            {
                return string.Empty;
            }

            var seatsFormatted = new StringBuilder();
            string currentRow = string.Empty; // Start with an empty string

            foreach (var seat in AvailableSeats)
            {
                string seatRow = seat.Substring(0, 1); // Get the row character as a string

                if (seatRow != currentRow)
                {
                    if (!string.IsNullOrEmpty(currentRow))
                    {
                        // Add the accumulated seats of the previous row to the formatted string
                        seatsFormatted.AppendLine();
                    }

                    // Start a new row
                    currentRow = seatRow;
                }

                seatsFormatted.Append($"{seat} ");
            }

            return seatsFormatted.ToString().TrimEnd(); // Remove any trailing newline characters
        }

        /// <summary>
        /// Update the available seats of the flight
        /// </summary>
        /// <param name="seat">Seat</param>
        /// <param name="removeSeat">Seat to be removed?</param>
        public void UpdateAvailableSeats(string seat, bool removeSeat)
        {
            if (removeSeat)
            {
                AvailableSeats.Remove(seat);
            }
            else
            {
                AvailableSeats.Add(seat);
            }

            UpdatePassengerCount();
        }

        /// <summary>
        /// Update the ticket list of the flight
        /// </summary>
        /// <param name="ticket">Ticket</param>
        /// <param name="removeTicket">Ticket to be removed?</param>
        public void UpdateTicketList(Ticket ticket, bool removeTicket)
        {
            string seat = ticket.Seat;

            if (removeTicket)
            {
                UpdateAvailableSeats(seat, false);
                TicketList.Remove(ticket);
            }
            else
            {
                TicketList.Add(ticket);
                UpdateAvailableSeats(seat, true);
            }
        }

        /// <summary>
        /// Generate a price for the flight tickets
        /// </summary>
        /// <returns>Ticker price</returns>
        public decimal TicketPriceGenerator()
        {
            // Algorithm to generate a price based on the duration of the flight and seat rarity (Not applicable for real use, but for illustrating distinct pricing scenarios)
            int totalMinutes = (int)Duration.TotalMinutes;
            decimal basePrice = 25 + (totalMinutes / 10);
            decimal availableSeatsPercent = (decimal)AvailableSeats.Count / Aircraft.Capacity;

            decimal price = basePrice + (basePrice * (1 - availableSeatsPercent)) * 3;

            return price;
        }

        /// <summary>
        /// Update the passenger count property
        /// </summary>
        private void UpdatePassengerCount()
        {
            PassengerCount = TicketList.Count;
        }

        #endregion
    }
}
