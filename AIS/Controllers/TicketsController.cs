using AIS.Data;
using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Helpers;
using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Controllers
{
    public class TicketsController : Controller
    {
        private readonly DataContext _context;
        private readonly IFlightRepository _flightRepository;
        private readonly IUserHelper _userHelper;
        private readonly ITicketRepository _ticketRepository;

        public TicketsController(DataContext context, IFlightRepository flightRepository, IUserHelper userHelper, ITicketRepository ticketRepository)
        {
            _context = context;
            _flightRepository = flightRepository;
            _userHelper = userHelper;
            _ticketRepository = ticketRepository;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            User currentUser = await _userHelper.GetUserAsync(this.User);
            List<Ticket> userTickets = await _ticketRepository.GetTicketsByUser(currentUser.Id);
                
            return View(userTickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
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

            if (flight.AvailableSeats.Count == 0)
            {
                ViewBag.ShowMsg = true;
                ViewBag.State = "disabled";
                ViewBag.Message = "There are not seats left on this Flight!";

                return View();
            }

            BuyTicketViewModel model = new BuyTicketViewModel();

            model.FlightId = flight.Id;

            model.SeatsList = flight.AvailableSeats.Select(s => new SelectListItem
            {
                Value = s,
                Text = s,
            });

            var adultAge = DateTime.UtcNow.AddYears(-18);

            model.DateOfBirth = adultAge;
            model.Flight = flight;
            model.FlightNumber = flight.FlightNumber;
            model.OriginCityCountry = $"{flight.Origin.City}, {flight.Origin.Country}";
            model.DestinationCityCountry = $"{flight.Destination.City}, {flight.Destination.Country}";
            model.DepartureDate = flight.Departure;
            model.ArrivalDate = flight.Arrival;
            model.TicketPrice = flight.TicketPriceGenerator();

            return View(model);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("Purchase/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BuyTicketViewModel model)
        {
            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(model.FlightId); // Get flight with track to update it

            foreach (Ticket checkTicket in flight.TicketList)
            {
                if (checkTicket.IdNumber == model.IdNumber)
                {
                    ModelState.AddModelError("IdNumber", "There is already a ticket with this Identification Number in this Flight!");
                }
            }

            // Default these properties for next view load
            model.Flight = flight;
            model.FlightNumber = flight.FlightNumber;
            model.OriginCityCountry = $"{flight.Origin.City}, {flight.Origin.Country}";
            model.DestinationCityCountry = $"{flight.Destination.City}, {flight.Destination.Country}";
            model.DepartureDate = flight.Departure;
            model.ArrivalDate = flight.Arrival;
            model.TicketPrice = flight.TicketPriceGenerator();
            model.SeatsList = flight.AvailableSeats.Select(s => new SelectListItem
            {
                Value = s,
                Text = s,
            });

            if (ModelState.IsValid)
            {
                Ticket newTicket = new Ticket
                {
                    User = await _userHelper.GetUserAsync(this.User),
                    ContactNumber = model.ContactNumber,
                    Email = model.Email,
                    DateOfBirth = model.DateOfBirth,
                    Flight = flight,
                    Seat = model.Seat,
                    Title = model.Title,
                    FullName = model.FullName,
                    IdNumber = model.IdNumber,
                };

                try
                {
                    // Empty these inputs for next ticket holder info
                    model.ContactNumber = string.Empty;
                    model.Email = string.Empty;
                    model.FullName = string.Empty;
                    model.IdNumber = string.Empty;

                    // Last check to see if seat is still available
                    bool ticketAvailable = false;
                    foreach (var seat in flight.AvailableSeats)
                    {
                        if (seat == model.Seat)
                        {
                            ticketAvailable = true;
                        }
                    }
                    if (ticketAvailable)
                    {
                        // Create the ticket in the database
                        await _ticketRepository.CreateAsync(newTicket);

                        // Update the flights internal ticketlist
                        flight.UpdateTicketList(newTicket, false);

                        // Update the flight in the database
                        await _flightRepository.UpdateAsync(flight);

                        return View(model);
                    }
                    else
                    {
                        ViewBag.ShowMsg = true;
                        return View(model);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    ViewBag.ShowMsg = true;
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Seat,Title,FullName,IdNumber,ContactNumber,Email,DateOfBirth")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}
