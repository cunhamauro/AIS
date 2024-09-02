using AIS.Data;
using AIS.Data.Entities;
using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public async Task<IActionResult> Index()
        {
            var flights = await _flightRepository.GetFlightsIncludeAsync();
            return View(flights);
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
