using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IImagesAPIService _imagesAPIService;
        private readonly IConfiguration _configuration;
        private readonly int numImagesGallery;

        public HomeController(ILogger<HomeController> logger, IFlightRepository flightRepository, IAirportRepository airportRepository, IImagesAPIService imagesAPIService, IConfiguration configuration)
        {
            _logger = logger;
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
            _imagesAPIService = imagesAPIService;
            _configuration = configuration;

            numImagesGallery = int.Parse(_configuration["AppSettings:NumberImagesGallery"]);
        }

        [HttpGet]
        [Route("Gallery/{id}")]
        public async Task<IActionResult> Gallery(int? id)
        {
            if (id == null)
            {
                return PageNotFound();
            }

            Airport airport = await _airportRepository.GetByIdAsync(id.Value);

            if (airport == null)
            {
                return PageNotFound();
            }

            // United+Arab+Emirates for image search 
            string searchCountry = airport.Country.Replace(" ", "+");

            List<string> listUrls = await _imagesAPIService.GetCountryImageUrl(searchCountry); // Image urls fetched through Google Search API

            CountryGalleryViewModel model = new CountryGalleryViewModel
            {
                CountryName = airport.Country,
                ImageUrls = new List<string>(),
            };

            for (int i = 0; i < numImagesGallery; i++) // Add the number of images we want to display (5 default)
            {
                model.ImageUrls.Add(listUrls[i]);
            }

            return View(model);
        }

        public async Task<IActionResult> Index(FlightsFiltersModelView model)
        {
            // Get all flights initially
            var flights = await _flightRepository.GetFlightsTrackIncludeAsync();

            // Get only the flights with seats available
            flights = flights.Where(f => f.AvailableSeats.Count > 0).ToList(); // TODO CHECK IF FLIGHT ONLY WITH SEATS FILTER FUNCTIONS HOME CONTROLLER INDEX

            FlightsFiltersModelView flightsModel = new FlightsFiltersModelView();

            if (flights != null && flights.Any())
            {
                // Get the current time if times are not filtered in the view
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
                    flights = flights.Where(f => f.Departure > model.Departure).ToList();
                    flightsModel.Departure = model.Departure; // If departure is filtered, set it
                }

                if (model.FilterByArrival)
                {
                    flights = flights.Where(f => f.Arrival < model.Arrival).ToList();
                    flightsModel.Arrival = model.Arrival; // If arrival is filtered, set it
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
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Something went wrong", Message = $"Get your parachute!" });
        }

        public IActionResult PageNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Page not found", Message = $"Need a ticket to go back?" });
        }
    }
}
