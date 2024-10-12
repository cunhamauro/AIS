using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Helpers;
using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IUserHelper _userHelper;
        private readonly ITicketRepository _ticketRepository;
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IFlightRecordRepository _flightRecordRepository;
        private readonly ITicketRecordRepository _ticketRecordRepository;

        private readonly int numImagesGallery;

        public HomeController(ILogger<HomeController> logger, IFlightRepository flightRepository, IAirportRepository airportRepository, IImagesAPIService imagesAPIService, IConfiguration configuration, IUserHelper userHelper, ITicketRepository ticketRepository, IAircraftRepository aircraftRepository, IFlightRecordRepository flightRecordRepository, ITicketRecordRepository ticketRecordRepository)
        {
            _logger = logger;
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
            _imagesAPIService = imagesAPIService;
            _configuration = configuration;
            _userHelper = userHelper;
            _ticketRepository = ticketRepository;
            _aircraftRepository = aircraftRepository;
            _flightRecordRepository = flightRecordRepository;
            _ticketRecordRepository = ticketRecordRepository;

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

        public async Task<IActionResult> Dashboard()
        {
            List<Flight> flights = await _flightRepository.GetFlightsTrackIncludeAsync();
            Dictionary<Airport, int> destinationCounts = new Dictionary<Airport, int>();

            foreach (Flight flight in flights)
            {
                if (destinationCounts.ContainsKey(flight.Destination)) // If the airport is already there
                {
                    destinationCounts[flight.Destination]++; // Add 1 more
                }
                else
                {
                    destinationCounts[flight.Destination] = 1; // Start it
                }
            }

            Airport popularDestination = new Airport();
            int maxCount = 0;

            foreach (var kvp in destinationCounts)
            {
                if (kvp.Value > maxCount)
                {
                    popularDestination = kvp.Key;
                    maxCount = kvp.Value;
                }
            }

            List<UserWithRolesViewModel> users = await _userHelper.GetUsersIncludeRolesAsync();

            int adminsCount = 0;
            int clientsCount = 0;
            int employeesCount = 0;

            foreach (var user in users)
            {
                if (user.Roles.FirstOrDefault() == "Admin")
                {
                    adminsCount++;
                }
                else if (user.Roles.FirstOrDefault() == "Client")
                {
                    clientsCount++;
                }
                else if (user.Roles.FirstOrDefault() == "Employee")
                {
                    employeesCount++;
                }
            }

            int flightsCount = _flightRepository.GetAll().Count();
            List<Ticket> tickets = await _ticketRepository.GetAll().ToListAsync();
            int airportsCount = _airportRepository.GetAll().Count();
            int aircraftsCount = _aircraftRepository.GetAll().Count();
            List<FlightRecord> flightRecords = await _flightRecordRepository.GetAll().ToListAsync();
            List<TicketRecord> ticketRecords = await _ticketRecordRepository.GetAllNonCanceledTicketRecords();

            decimal moneyTickets = 0;
            foreach (var ticket in tickets)
            {
                moneyTickets += ticket.Price;
            }

            decimal moneyTotalTickets = 0;
            foreach (var ticket in ticketRecords)
            {
                moneyTotalTickets += ticket.TicketPrice;
            }

            int canceledFlights = 0;
            foreach (var record in flightRecords)
            {
                if (record.Canceled)
                {
                    canceledFlights++;
                }
            }

            DashboardViewModel dashboard = new DashboardViewModel
            {
                MostPopularDestination = popularDestination,
                AdminsCount = adminsCount,
                EmployeesCount = employeesCount,
                ClientsCount = clientsCount,
                ActiveFlightsCount = flightsCount,
                ActiveTicketsCount = tickets.Count,
                AirportsCount = airportsCount,
                AircraftsCount = aircraftsCount,
                MoneyTotalTickets = moneyTotalTickets,
                TicketRecordsCount = ticketRecords.Count,
                FlightRecordsCount = flightRecords.Count,
                CanceledFlightsCount = canceledFlights,
            };

            if (flightsCount == 0)
            {
                ViewBag.AvailableDestination = false;
            }
            else
            {
                ViewBag.AvailableDestination = true;
            }

            return View(dashboard);
        }

        public async Task<IActionResult> Index(FlightsFiltersViewModel model)
        {
            User user = await _userHelper.GetUserAsync(this.User);

            if (user == null || await _userHelper.IsUserInRoleAsync(user, "Client"))
            {
                ViewBag.IsClient = true;
            }

            // Get all flights initially
            var flights = await _flightRepository.GetFlightsTrackIncludeAsync();

            DateTime now = DateTime.UtcNow.AddMinutes(30);

            // Get only the flights with seats available and that have a departure date with at least 30min left
            flights = flights.Where(f => f.AvailableSeats.Count > 0).Where(f => f.Departure > now).ToList();

            FlightsFiltersViewModel flightsModel = new FlightsFiltersViewModel();

            // Get the current time if times are not filtered in the view
            flightsModel.Departure = DateTime.UtcNow.AddHours(1);
            flightsModel.Arrival = DateTime.UtcNow.AddHours(2);

            if (flights != null && flights.Any())
            {
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
