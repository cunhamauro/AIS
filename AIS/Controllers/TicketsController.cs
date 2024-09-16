using AIS.Data;
using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Helpers;
using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Controllers
{
    public class TicketsController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IUserHelper _userHelper;
        private readonly ITicketRepository _ticketRepository;
        private readonly IConfiguration _configuration;
        private readonly IConverterHelper _converterHelper;
        private readonly int marginTicketCancelation;

        public TicketsController(IFlightRepository flightRepository, IUserHelper userHelper, ITicketRepository ticketRepository, IConfiguration configuration, IConverterHelper converterHelper)
        {
            _flightRepository = flightRepository;
            _userHelper = userHelper;
            _ticketRepository = ticketRepository;
            _configuration = configuration;
            _converterHelper = converterHelper;
            marginTicketCancelation = int.Parse(_configuration["AppSettings:TicketCancelationMarginHours"]);
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            User currentUser = await _userHelper.GetUserAsync(this.User);
            List<Ticket> userTickets = await _ticketRepository.GetTicketsByUserIncludeFlightAirportsAsync(currentUser.Id);
                
            return View(userTickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var ticket = await _ticketRepository.GetTicketIncludeFlightAirportsAsync(id.Value);

            if (ticket == null)
            {
                return TicketNotFound();
            }

            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(ticket.Flight.Id); // Get flight with track to update it

            if (flight == null)
            {
                return RedirectToAction("FlightNotFound", "Flights");
            }

            TicketViewModel model = _converterHelper.ToTicketViewModel(ticket, flight, null);

            return View(model);
        }

        // GET: Tickets/Create
        [HttpGet("Purchase/{id}")]
        public async Task<IActionResult> Create(int id)
        {
            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(id);

            if (flight == null)
            {
                return RedirectToAction("FlightNotFound", "Flights");
            }

            // Assure users access a valid flight that as not started boarding yet
            if (flight.Departure < DateTime.UtcNow.AddMinutes(30))
            {
                return RedirectToAction("FlightNotFound", "Flights");
            }
            
            if (flight.AvailableSeats.Count == 0) //TODO TESTAR SEM LUGARES
            {
                ViewBag.ShowMsg = true;
                ViewBag.State = "disabled";
                ViewBag.Message = "There are no seats left on this Flight!";

                return View();
            }

            TicketViewModel model = new TicketViewModel();

            model.FlightId = flight.Id;

            model.SeatsList = flight.AvailableSeats.Select(s => new SelectListItem
            {
                Value = s,
                Text = s,
            });

            // Set date time picker to 18 years old birthday (UX)
            var adultAge = DateTime.UtcNow.AddYears(-18);

            model.DateOfBirth = adultAge;
            model.Flight = flight;
            model.FlightNumber = flight.FlightNumber;
            model.OriginCityCountry = $"{flight.Origin.City}, {flight.Origin.Country}";
            model.DestinationCityCountry = $"{flight.Destination.City}, {flight.Destination.Country}";
            model.DepartureDate = flight.Departure;
            model.ArrivalDate = flight.Arrival;
            model.Price = flight.TicketPriceGenerator();

            return View(model);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("Purchase/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketViewModel model)
        {
            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(model.FlightId); // Get flight with track to update it

            if (flight.AvailableSeats.Count == 0)
            {
                ModelState.AddModelError("Seats", "There are no seats left on this Flight!");
            }

            foreach (Ticket checkTicket in flight.TicketList)
            {
                if (checkTicket.IdNumber == model.IdNumber)
                {
                    ModelState.AddModelError("IdNumber", "There is already a ticket with this Identification Number in this Flight!");
                }
            }

            if (ModelState.IsValid)
            {
                Ticket newTicket = new Ticket
                {
                    User = await _userHelper.GetUserAsync(this.User),
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    DateOfBirth = model.DateOfBirth,
                    Flight = flight,
                    Seat = model.Seat,
                    Title = model.Title,
                    FullName = model.FullName,
                    IdNumber = model.IdNumber,
                    Price = model.Price,
                };

                try
                {
                    // Last check to see if seat is still available
                    if (flight.AvailableSeats.Contains(newTicket.Seat))
                    {
                        // Create the ticket in the database
                        await _ticketRepository.CreateAsync(newTicket);

                        // Update the flights internal ticketlist
                        flight.UpdateTicketList(newTicket, false);

                        // Update the flight in the database
                        await _flightRepository.UpdateAsync(flight);

                        return RedirectToAction("Create", new { id = flight.Id });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Pass to next
                }
            }

            // Default these properties for next view load
            model.Flight = flight;
            model.FlightNumber = flight.FlightNumber;
            model.OriginCityCountry = $"{flight.Origin.City}, {flight.Origin.Country}";
            model.DestinationCityCountry = $"{flight.Destination.City}, {flight.Destination.Country}";
            model.DepartureDate = flight.Departure;
            model.ArrivalDate = flight.Arrival;

            model.Price = flight.TicketPriceGenerator();
            model.SeatsList = flight.AvailableSeats.Select(s => new SelectListItem
            {
                Value = s,
                Text = s,
            });

            ViewBag.ShowMsg = true;

            return View(model);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return TicketNotFound();
            }

            var ticket = await _ticketRepository.GetTicketIncludeFlightAirportsAsync(id.Value);

            if (ticket == null)
            {
                return TicketNotFound();
            }

            Flight flight = ticket.Flight;

            if (flight == null)
            {
                return RedirectToAction("FlightNotFound", "Flights");
            }

            // Assure users access a valid flight that as not started boarding yet
            if (flight.Departure < DateTime.UtcNow.AddMinutes(30))
            {
                ViewBag.ShowMsgFlight = true;
                ViewBag.State = "disabled";
            }

            //model.FlightId = ticket.Flight.Id;
            var listSeats = flight.AvailableSeats.Select(s => new SelectListItem
            {
                Value = s,
                Text = s,
            });

            SelectListItem takenSeat = new SelectListItem
            {
                Value = ticket.Seat,
                Text = ticket.Seat,
            };

            listSeats.ToList().Add(takenSeat);

            TicketViewModel model = _converterHelper.ToTicketViewModel(ticket, flight, listSeats.ToList());

            return View(model);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TicketViewModel model)
        {
            var ticket = await _ticketRepository.GetByIdTrackAsync(model.Id);

            if (ticket == null)
            {
                return TicketNotFound();
            }

            var oldSeat = ticket.Seat;

            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(model.FlightId); // Get flight with track to update it

            if (flight == null)
            {
                return RedirectToAction("FlightNotFound", "Flights");
            }

            if (ticket.IdNumber != model.IdNumber && flight.TicketList.Any(f => f.IdNumber == model.IdNumber))
            {
                ModelState.AddModelError("IdNumber", "There is already a ticket with this Identification Number in this Flight!");
            }

            if (flight.AvailableSeats.Count == 0)
            {
                ModelState.AddModelError("Seats", "There are no seats left on this Flight!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.FullName = model.FullName;
                    ticket.Email = model.Email;
                    ticket.PhoneNumber = model.PhoneNumber;
                    ticket.DateOfBirth = model.DateOfBirth;
                    ticket.IdNumber = model.IdNumber;
                    ticket.Title = model.Title;
                    ticket.Seat = model.Seat;

                    // Last check to see if seat is still available
                    if (flight.AvailableSeats.Contains(ticket.Seat))
                    {
                        // Update the ticket in the database
                        await _ticketRepository.UpdateAsync(ticket);

                        // Update the flights available seats
                        flight.UpdateAvailableSeats(ticket.Seat, true);
                        flight.UpdateAvailableSeats(oldSeat, false);

                        // Update the flight in the database
                        await _flightRepository.UpdateAsync(flight);

                        return RedirectToAction("Index");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Pass to next
                }
            }

            var listSeats = flight.AvailableSeats.Select(s => new SelectListItem
            {
                Value = s,
                Text = s,
            });

            SelectListItem takenSeat = new SelectListItem
            {
                Value = ticket.Seat,
                Text = ticket.Seat,
            };

            listSeats.ToList().Add(takenSeat);

            TicketViewModel reModel = _converterHelper.ToTicketViewModel(ticket, flight, listSeats.ToList());

            ViewBag.ShowMsgSeat = true;

            return View(reModel);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return TicketNotFound();
            }

            var ticket = await _ticketRepository.GetTicketIncludeFlightAirportsAsync(id.Value);

            Flight flight = ticket.Flight;

            if (ticket == null)
            {
                return TicketNotFound();
            }

            if (flight == null)
            {
                return RedirectToAction("FlightNotFound", "Flights");
            }

            // Only allow to cancel flights N until hours in advance
            if (flight.Departure < DateTime.UtcNow.AddHours(marginTicketCancelation))
            {
                ViewBag.ShowMsg = true;
                ViewBag.State = "disabled";
            }

            TicketViewModel model = _converterHelper.ToTicketViewModel(ticket, flight, null);

            return View(model);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _ticketRepository.GetTicketIncludeFlightAirportsAsync(id);

            if (ticket == null)
            {
                return TicketNotFound();
            }

            var seat = ticket.Seat;

            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(ticket.Flight.Id); // Get flight with track to update it

            if (flight == null)
            {
                return RedirectToAction("FlightNotFound", "Flights");
            }

            // Only allow to cancel flights N until hours in advance
            if (flight.Departure < DateTime.UtcNow.AddHours(marginTicketCancelation))
            {
                return RedirectToAction("Delete", "Tickets");
            }

            // Update the ticket in the database
            await _ticketRepository.DeleteAsync(ticket);

            // Update the flights internal ticketlist and available seats
            flight.UpdateTicketList(ticket, true);

            // Update the flight in the database
            await _flightRepository.UpdateAsync(flight);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult TicketNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Ticket not found", Message = $"Maybe search in your baggage!" });
        }
    }
}
