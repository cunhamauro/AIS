using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Helpers;
using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public FlightsController(IAirportRepository airportRepository, IAircraftRepository aircraftRepository, IFlightRepository flightRepository, IConverterHelper converterHelper, IUserHelper userHelper, IAircraftAvailabilityService aircraftAvailabilityService)
        {
            _airportRepository = airportRepository;
            _aircraftRepository = aircraftRepository;
            _flightRepository = flightRepository;
            _converterHelper = converterHelper;
            _userHelper = userHelper;
            _aircraftAvailabilityService = aircraftAvailabilityService;
        }

        // GET: Flights
        public IActionResult Index()
        {
            return View(_flightRepository.GetAll().Include(o => o.Origin).Include(d => d.Destination).Include(a => a.Aircraft)); // Show all Flights and nested Entities
        }

        // GET: Flights/FlightInformation/5
        public async Task<IActionResult> FlightInformation(int? id)
        {
            if (id == null)
            {
                return FlightNotFound();
            }

            Flight flight = await _flightRepository.GetFlightIncludeByIdAsync(id.Value);

            if (flight == null)
            {
                return FlightNotFound();
            }

            return View(flight);
        }

        // GET: Flights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return FlightNotFound();
            }

            Flight flight = await _flightRepository.GetFlightIncludeByIdAsync(id.Value);

            if (flight == null)
            {
                return FlightNotFound();
            }

            return View(flight);
        }

        // GET: Flights/Create
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

            var currentUser = await _userHelper.GetUserAsync(User);
            model.User = currentUser;

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

            return RedirectToAction(nameof(Index));
        }

        // GET: Flights/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return FlightNotFound();
            }

            Flight flight = await _flightRepository.GetFlightIncludeByIdAsync(id.Value);

            if (flight == null)
            {
                return FlightNotFound();
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return FlightNotFound();
            }

            Flight flight = await _flightRepository.GetFlightIncludeByIdAsync(id.Value);

            if (flight == null)
            {
                return FlightNotFound();
            }

            return View(flight);
        }

        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Flight flight = await _flightRepository.GetByIdAsync(id);

            await _flightRepository.DeleteAsync(flight);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult FlightNotFound()
        {
            return View("Error", new ErrorViewModel { ErrorMessage = "Flight not found!", RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [Route("Flight/GetAvailableAircrafts")]
        public async Task<JsonResult> GetAvailableAircrafts(DateTime departure, DateTime arrival, int originId)
        {
            Airport origin = await _airportRepository.GetByIdAsync(originId);

            if (origin != null && origin.Id > 0)
            {
                List<Aircraft> availableAircrafts = await _aircraftAvailabilityService.AvailableAircrafts(departure, arrival, origin);
                return Json(availableAircrafts);
            }
            else
            {
                return Json(new List<Aircraft>()); // Return an empty list if the origin is invalid
            }
        }
    }
}
