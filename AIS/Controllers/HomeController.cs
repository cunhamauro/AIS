using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Syncfusion.EJ2.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;

        public HomeController(ILogger<HomeController> logger, IFlightRepository flightRepository, IAirportRepository airportRepository)
        {
            _logger = logger;
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
        }

        [HttpGet]
        [Route("Gallery/{id}")]
        public async Task<IActionResult> Gallery(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            Airport airport = await _airportRepository.GetByIdAsync(id.Value);

            if (airport == null)
            {
                return RedirectToAction("Index");
            }

            return View(airport);
        }

        public async Task<IActionResult> Index(FlightsFiltersModelView model)
        {
            // Get all flights initially
            var flights = await _flightRepository.GetFlightsIncludeAsync();
            FlightsFiltersModelView flightsModel = new FlightsFiltersModelView();

            if (flights != null && flights.Any())
            {
                flightsModel.Departure = DateTime.Now;
                flightsModel.Arrival = DateTime.Now.AddHours(1);

                // Apply filters if selected
                if (model.FilterByOrigin && model.OriginId > 0)
                {
                    flights = flights.Where(f => f.Origin.Id == model.OriginId).ToList();
                }

                if (model.FilterByDestination && model.DestinationId > 0)
                {
                    flights = flights.Where(f => f.Destination.Id == model.DestinationId).ToList();
                }

                if (model.FilterByDeparture)
                {
                    flights = flights.Where(f => f.Departure.Date > model.Departure.Date).ToList();
                    flightsModel.Departure = model.Departure;
                }

                if (model.FilterByArrival)
                {
                    flights = flights.Where(f => f.Arrival.Date < model.Arrival.Date).ToList();
                    flightsModel.Arrival = model.Arrival;
                }

                // Prepare the model to be passed to the view
                flightsModel.Flights = flights;
                flightsModel.OriginList = await _airportRepository.AirportSelectionList();
                flightsModel.DestinationList = await _airportRepository.AirportSelectionList();
                flightsModel.OriginId = model.OriginId;
                flightsModel.DestinationId = model.DestinationId;
                flightsModel.FilterByOrigin = model.FilterByOrigin;
                flightsModel.FilterByDestination = model.FilterByDestination;
                flightsModel.FilterByDeparture = model.FilterByDeparture;
                flightsModel.FilterByArrival = model.FilterByArrival;
            }

            return View(flightsModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error", new ErrorViewModel { ErrorMessage = "Something went wrong!", RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult PageNotFound()
        {
            return View("Error", new ErrorViewModel { ErrorMessage = "Page not found!", RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
