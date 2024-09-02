using AIS.Data;
using AIS.Data.Entities;
using AIS.Helpers;
using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AIS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AirportsController : Controller
    {
        private readonly IAirportRepository _airportRepository;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IAirportsAPIService _airportsService;
        private readonly IFlightRepository _flightRepository;
        private readonly ICountriesAPIService _countriesService;

        public AirportsController(IAirportRepository airportRepository, IUserHelper userHelper, IConverterHelper converterHelper, IImageHelper imageHelper, ICountriesAPIService countriesService, IAirportsAPIService airportsService, IFlightRepository flightRepository)
        {
            _airportRepository = airportRepository;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _airportsService = airportsService;
            _flightRepository = flightRepository;
            _countriesService = countriesService;
        }

        // GET: Airports
        public IActionResult Index()
        {
            return View(_airportRepository.GetAll());
        }

        // GET: Airports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return AirportNotFound();
            }

            Airport airport = await _airportRepository.GetByIdAsync(id.Value);

            if (airport == null)
            {
                return AirportNotFound();
            }

            return View(airport);
        }

        // GET: Airports/Create
        public async Task<IActionResult> Create()
        {
            List<Airport> listAirports = await _airportsService.GetAirportsAsync();

            List<SelectListItem> selectAirportList = new List<SelectListItem>();

            foreach (Airport airport in listAirports)
            {
                selectAirportList.Add(new SelectListItem
                {
                    Value = airport.IATA,
                    Text = $"{airport.IATA} - {airport.City}, {airport.Country}",
                    // Decision to leave order by IATA because professionals won't search by City or Country
                });
            }

            var viewModel = new AirportViewModel
            {
                IataList = selectAirportList,
            };

            return View(viewModel);
        }

        // POST: Airports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirportViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.IATA))
            {
                ModelState.AddModelError("IATA", "Please select a valid airport!");
            }

            if (await _airportRepository.IataExistsAsync(viewModel.IATA))
            {
                ModelState.AddModelError($"IATA", $"[{viewModel.IATA}] is already part of the list of active Airports!");
            }

            List<Airport> listAirports = await _airportsService.GetAirportsAsync(); // Get the list of Airports from API
            List<Country> listCountriesAPI = await _countriesService.GetCountriesAsync(); // Get the list of Countries from API

            if (!ModelState.IsValid)
            {
                #region IataList Reassignment

                // If the page is refreshed and viewModel returned

                List<SelectListItem> listAirportsSelect = new List<SelectListItem>();

                foreach (Airport a in listAirports)
                {
                    listAirportsSelect.Add(new SelectListItem
                    {
                        Value = a.IATA,
                        Text = $"{a.IATA} - {a.City}, {a.Country}",
                    });
                }

                viewModel.IataList = listAirportsSelect;

                #endregion

                return View(viewModel);
            }

            // Convert view model to airport
            Airport airport = _converterHelper.ToAirport(viewModel.IATA, listAirports.FirstOrDefault(a => a.IATA == viewModel.IATA).Country, listAirports.FirstOrDefault(a => a.IATA == viewModel.IATA).City); ;

            // Assign user to the created airport
            var currentUser = await _userHelper.GetUserAsync(User);

            //if (currentUser == null)
            //{
            //    //return 
            //}

            // Get the image flag url
            string imageUrl = listCountriesAPI.FirstOrDefault(c => c.Name.Common == airport.Country).Flags.Png;

            airport.User = currentUser; // Assign the current user to the aircraft
            airport.ImageUrl = imageUrl;

            await _airportRepository.CreateAsync(airport);

            return RedirectToAction(nameof(Index));
        }

        #region Deactivated EDIT of Airport

        //// GET: Airports/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    Airport airport = await _airportRepository.GetByIdAsync(id.Value);

        //    if (airport == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(airport);
        //}

        //// POST: Airports/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, Airport airport)
        //{
        //    if (id != airport.Id)
        //    {
        //        return NotFound();
        //    }

        //    Airport oldAirport = await _airportRepository.GetByIdAsync(id);

        //    if (await _airportRepository.IataExistsAsync(oldAirport.IATA, airport.IATA))
        //    {
        //        ModelState.AddModelError($"IATA", $"This IATA is already assigned to another Airport!");
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            await _airportRepository.UpdateAsync(airport);
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!await _airportRepository.ExistAsync(airport.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(airport);
        //}

        #endregion

        // GET: Airports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return AirportNotFound();
            }

            Airport airport = await _airportRepository.GetByIdAsync(id.Value);

            if (airport == null)
            {
                return AirportNotFound();
            }

            return View(airport);
        }

        // POST: Airports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Airport airport = await _airportRepository.GetByIdAsync(id);

            if (await _flightRepository.AirportInFlights(id))
            {
                ViewBag.ShowMsg = true;
                return View(airport);
            }

            await _airportRepository.DeleteAsync(airport);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult AirportNotFound()
        {
            return View("Error", new ErrorViewModel { ErrorMessage = "Airport not found!", RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
