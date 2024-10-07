using AIS.Data.Classes;
using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Helpers;
using AIS.Migrations;
using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Controllers
{
    public class FlightsController : Controller
    {
        private readonly IAirportRepository _airportRepository;
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IUserHelper _userHelper;
        private readonly IAircraftAvailabilityService _aircraftAvailabilityService;
        private readonly IConfiguration _configuration;
        private readonly IMailHelper _mailHelper;
        private readonly IPdfHelper _pdfHelper;
        private readonly ITicketRecordRepository _ticketRecordRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IFlightRecordRepository _flightRecordRepository;

        private readonly int marginBetweenFlights;

        public FlightsController(IAirportRepository airportRepository, IAircraftRepository aircraftRepository, IFlightRepository flightRepository, IConverterHelper converterHelper, IUserHelper userHelper, IAircraftAvailabilityService aircraftAvailabilityService, IConfiguration configuration, IMailHelper mailHelper, IPdfHelper pdfHelper, ITicketRecordRepository ticketRecordRepository, ITicketRepository ticketRepository, IFlightRecordRepository flightRecordRepository)
        {
            _airportRepository = airportRepository;
            _aircraftRepository = aircraftRepository;
            _flightRepository = flightRepository;
            _converterHelper = converterHelper;
            _userHelper = userHelper;
            _aircraftAvailabilityService = aircraftAvailabilityService;
            _configuration = configuration;
            _mailHelper = mailHelper;
            _pdfHelper = pdfHelper;
            _ticketRecordRepository = ticketRecordRepository;
            _ticketRepository = ticketRepository;
            _flightRecordRepository = flightRecordRepository;

            marginBetweenFlights = int.Parse(_configuration["AppSettings:MarginMinutesBetweenFlights"]);
        }

        // GET: Flights
        [Authorize(Roles = "Employee")]
        public IActionResult Index()
        {
            return View(_flightRepository.GetAll().Include(o => o.Origin).Include(d => d.Destination).Include(a => a.Aircraft)); // Show all Flights and nested Entities
        }

        // GET: Flights/UserFlights
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> UserFlights()
        {
            List<TicketRecord> ticketRecords = await _ticketRecordRepository.GetAll().ToListAsync();
            User user = await _userHelper.GetUserAsync(this.User);
            ticketRecords = ticketRecords.Where(f => f.UserId == user.Id).ToList();

            return View(ticketRecords);
        }

        // GET: Flights/FlightRecords
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> FlightRecords()
        {
            List<FlightRecord> flightRecords = await _flightRecordRepository.GetAll().ToListAsync();

            return View(flightRecords);
        }

        // GET: Flights/FlightInformation/5
        public async Task<IActionResult> FlightInformation(int? id)
        {
            if (id == null)
            {
                return FlightNotFound();
            }

            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(id.Value);

            if (flight == null)
            {
                return FlightNotFound();
            }

            // Calculate seats info
            int availableSeats = flight.AvailableSeats.Count;
            int capacity = flight.Aircraft.Capacity;

            // Handle division by zero to avoid errors
            decimal availability = (capacity > 0) ? (decimal)availableSeats / capacity * 100 : 0;
            ViewBag.SeatsInfo = $"{availableSeats} / {capacity} ({availability.ToString("0")} %)";

            return View(flight);
        }

        // GET: Flights/Details/5
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return FlightNotFound();
            }

            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(id.Value);

            if (flight == null)
            {
                return FlightNotFound();
            }

            return View(flight);
        }

        // GET: Flights/Create
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Create()
        {
            // Return the model with a select list of aircrafts, list of airports for origin and destination)
            return View(new FlightViewModel
            {
                AircraftList = await _aircraftRepository.AircraftSelectionList(),
                OriginList = await _airportRepository.AirportSelectionList(),
                DestinationList = await _airportRepository.AirportSelectionList(),
            });
        }

        // POST: Flights/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlightViewModel model) // Receive the filled model (will only have INT IDs for the entities it needs)
        {
            // Find the entities through the IDs stored in the model using tracking to save the relations between Flight and rest of entities
            Airport origin = await _airportRepository.GetByIdTrackAsync(model.OriginId);
            Airport destination = await _airportRepository.GetByIdTrackAsync(model.DestinationId);
            Aircraft aircraft = await _aircraftRepository.GetByIdTrackAsync(model.AircraftId);

            if (origin.IATA == destination.IATA)
            {
                ModelState.AddModelError("DestinationId", "Flight origin and destination must be different!");
            }

            if (model.Departure >= model.Arrival)
            {
                ModelState.AddModelError("Arrival", "Flight arrival must be after the departure date!");
            }

            if (model.Departure <= DateTime.Now.AddHours(24)) // 24h minimum to schedule a flight
            {
                ModelState.AddModelError("Departure", "Flights must be scheduled 24 hours in advance!");
            }

            if (!ModelState.IsValid)
            {
                // Return the model with the select lists repopulated
                return View(new FlightViewModel
                {
                    AircraftList = await _aircraftRepository.AircraftSelectionList(),
                    OriginList = await _airportRepository.AirportSelectionList(),
                    DestinationList = await _airportRepository.AirportSelectionList(),
                });
            }

            model.User = await _userHelper.GetUserAsync(User);

            // Convert the model to a flight
            Flight flight = _converterHelper.ToFlight(aircraft, origin, destination, model, true);

            await _flightRepository.CreateAsync(flight); // Create the flight

            // Retrieve the saved flight to get its ID given by the DB
            Flight savedFlight = await _flightRepository.GetByIdAsync(flight.Id);
            int savedFlightId = savedFlight.Id;

            // And generate the flight number
            flight.GenerateFlightNumber(savedFlightId, origin.IATA, destination.IATA);

            // Update it to save the flight number
            await _flightRepository.UpdateAsync(flight);

            // Create a flight record
            FlightRecord flightRecord = new FlightRecord
            {
                Id = savedFlightId,
                FlightNumber = flight.FlightNumber,
                OriginCity = flight.Origin.City,
                OriginCountry = flight.Origin.Country,
                OriginFlagImageUrl = flight.Origin.ImageUrl,
                DestinationCity = flight.Destination.City,
                DestinationCountry = flight.Destination.Country,
                DestinationFlagImageUrl = flight.Destination.ImageUrl,
                Departure = flight.Departure,
                Arrival = flight.Arrival,
                Canceled = false,
            };
            await _flightRecordRepository.CreateAsync(flightRecord);

            return RedirectToAction(nameof(Index));
        }

        // GET: Flights/Edit/5
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return FlightNotFound();
            }

            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(id.Value);

            if (flight == null)
            {
                return FlightNotFound();
            }

            if (flight.Departure.AddHours(24) < DateTime.UtcNow)
            {
                return DisplayMessage("Flight Update Disabled", "Flight can only be updated minimum 24 hours before departure!");
            }

            FlightViewModel model = _converterHelper.ToFlightViewModel(flight);
            model.AircraftList = await _aircraftRepository.AircraftSelectionList();

            model.AircraftList = model.AircraftList.Select(a => new SelectListItem
            {
                Value = a.Value,
                Text = a.Text,
                Selected = a.Value == model.AircraftId.ToString()
            });

            return View(model);
        }

        // POST: Flights/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FlightViewModel model)
        {
            if (model.Departure >= model.Arrival)
            {
                ModelState.AddModelError("Arrival", "Flight arrival must be after the departure date!");
            }

            if (model.Departure <= DateTime.Now.AddHours(24)) // 24h minimum to schedule a flight
            {
                ModelState.AddModelError("Departure", "Flights must be scheduled 24 hours in advance!");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate aircraft select list
                model.AircraftList = await _aircraftRepository.AircraftSelectionList();

                // Selected aircraft
                model.AircraftList = model.AircraftList.Select(a => new SelectListItem
                {
                    Value = a.Value,
                    Text = a.Text,
                    Selected = a.Value == model.AircraftId.ToString()
                });

                return View(model);
            }

            var flight = await _flightRepository.GetByIdAsync(model.Id); // Fetch flight with no tracking and no includes

            if (flight == null)
            {
                return FlightNotFound();
            }

            // Assign the updated properties
            flight.Departure = model.Departure;
            flight.Arrival = model.Arrival;

            // Fetch the aircraft through the ID with tracking to set relation to existing entity aircraft
            flight.Aircraft = await _aircraftRepository.GetByIdTrackAsync(model.AircraftId);
            flight.User = await _userHelper.GetUserAsync(User); // Set the current user that made the edit

            try
            {
                await _flightRepository.UpdateAsync(flight);

                // Update the corresponding flight record
                FlightRecord flightRecord = await _flightRecordRepository.GetByIdAsync(flight.Id);
                flightRecord.Departure = model.Departure;
                flightRecord.Arrival = model.Arrival;
                await _flightRecordRepository.UpdateAsync(flightRecord);

                // Update all ticket records flights related to this flight
                List<TicketRecord> ticketRecords = await _ticketRecordRepository.GetAll().ToListAsync();
                foreach (TicketRecord record in ticketRecords)
                {
                    if (record.FlightNumber == flight.FlightNumber)
                    {
                        record.Departure = model.Departure;
                        record.Arrival = model.Arrival;
                        await _ticketRecordRepository.UpdateAsync(record);
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _flightRepository.ExistAsync(model.Id))
                {
                    return FlightNotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Flights/Delete/5
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return FlightNotFound();
            }

            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(id.Value);

            if (flight == null)
            {
                return FlightNotFound();
            }

            if (flight.TicketList.Any())
            {
                ViewBag.ShowMsg = true;
            }

            return View(flight);
        }

        // POST: Flights/Delete/5
        [Authorize(Roles = "Employee")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Flight flight = await _flightRepository.GetFlightTrackIncludeByIdAsync(id);

            // If the flight is being canceled before it could be completed:
            if (flight.Departure > DateTime.UtcNow)
            {
                // Update the flight record to canceled
                FlightRecord flightRecord = await _flightRecordRepository.GetByIdAsync(id);
                flightRecord.Canceled = true;
                await _flightRecordRepository.UpdateAsync(flightRecord);

                // Update ticket records flights to canceled
                List<TicketRecord> ticketRecords = await _ticketRecordRepository.GetAll().ToListAsync();
                foreach (TicketRecord record in ticketRecords)
                {
                    record.Canceled = true;
                    await _ticketRecordRepository.UpdateAsync(record);
                }

                // Email and PDF for flight cancel/refund (100% back)
                User user = await _userHelper.GetUserAsync(this.User);

                // Flight will be canceled so all tickets should be fully refunded
                foreach (var ticket in flight.TicketList)
                {
                    string emailBodyTicket = _mailHelper.GetHtmlTemplateTicket("Flight Cancel", $"{ticket.Title} {ticket.FullName}", ticket.IdNumber, flight.FlightNumber, $"{flight.Origin.City}, {flight.Origin.Country}", $"{flight.Destination.City}, {flight.Destination.Country}", ticket.Seat, flight.Departure, flight.Arrival, true);
                    MemoryStream pdfInvoice = _pdfHelper.GenerateInvoicePdf($"{user.FirstName} {user.LastName}", flight.FlightNumber, ticket.Price, false, true);

                    // Makes sense to send an email to both ticket holder and ticket buyer, because one uses the ticket and the other buys it
                    Response responseTicketHolder = await _mailHelper.SendEmailAsync(ticket.Email, $"Cancel Flight ID-{id}", emailBodyTicket, pdfInvoice, $"flight_cancel_{flight.FlightNumber}_{ticket.IdNumber}.pdf", null);
                    Response responseTicketBuyer = await _mailHelper.SendEmailAsync(user.Email, $"Cancel Flight ID-{id}", emailBodyTicket, pdfInvoice, $"flight_cancel_{flight.FlightNumber}_{ticket.IdNumber}.pdf", null);

                    if (!responseTicketHolder.IsSuccess && !responseTicketBuyer.IsSuccess)
                    {
                        return DisplayMessage("Refund Mailing Error", $"The refund info failed to send to: {user.Email} & {ticket.Email}!");
                    }
                }

            }
            // If the flight is not being canceled, was completed, and is being deleted:

            // Create a list of tickets to delete
            var ticketsToDelete = flight.TicketList.ToList();

            // Delete the Tickets
            if (ticketsToDelete.Any())
            {
                foreach (Ticket ticketCancel in ticketsToDelete)
                {
                    await _ticketRepository.DeleteAsync(ticketCancel);
                }
            }

            await _flightRepository.DeleteAsync(flight);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("Flights/GetAvailableAircrafts")]
        public async Task<JsonResult> GetAvailableAircrafts(DateTime departure, DateTime arrival, int originId)
        {
            var currentDate = DateTime.Now;

            // If the dates are not valid, simply return an empty list of aircrafts
            if (departure < arrival && departure >= currentDate.AddMinutes(marginBetweenFlights) && arrival >= currentDate.AddMinutes(marginBetweenFlights))
            {
                Airport origin = await _airportRepository.GetByIdAsync(originId);

                if (origin != null && origin.Id > 0)
                {
                    List<Aircraft> availableAircrafts = await _aircraftAvailabilityService.AvailableAircrafts(departure, arrival, origin);
                    return Json(availableAircrafts);
                }
            }
            return Json(new List<Aircraft>());
        }

        [HttpPost]
        [Route("Flights/GetAvailableAircraftsEdit")]
        public async Task<JsonResult> GetAvailableAircraftsEdit(int flightId, DateTime departure, DateTime arrival, int originId)
        {
            var currentDate = DateTime.Now;

            // If the dates are not valid, simply return an empty list of aircrafts
            if (departure < arrival && departure >= currentDate.AddMinutes(marginBetweenFlights) && arrival >= currentDate.AddMinutes(marginBetweenFlights))
            {
                Airport origin = await _airportRepository.GetByIdAsync(originId);

                if (origin != null && origin.Id > 0)
                {
                    List<Aircraft> availableAircrafts = await _aircraftAvailabilityService.AvailableAircrafts(departure, arrival, origin);

                    Flight flightToEdit = await _flightRepository.GetFlightTrackIncludeByIdAsync(flightId);

                    Aircraft currentAircraft = flightToEdit.Aircraft;

                    if (await _aircraftAvailabilityService.AircraftEditAvailableOnDate(currentAircraft, departure, arrival, flightToEdit))
                    {
                        if (!availableAircrafts.Contains(currentAircraft))
                        {
                            availableAircrafts.Add(flightToEdit.Aircraft);
                        }
                    }
                    else
                    {
                        if (availableAircrafts.Contains(currentAircraft))
                        {
                            availableAircrafts.Remove(flightToEdit.Aircraft);
                        }
                    }
                    return Json(availableAircrafts);
                }
            }
            return Json(new List<Aircraft>());
        }

        public IActionResult DisplayMessage(string title, string message)
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = $"{title}", Message = $"{message}" });
        }

        public IActionResult FlightNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Flight not found", Message = "You didn't make it to the gate in time..." });
        }
    }
}
