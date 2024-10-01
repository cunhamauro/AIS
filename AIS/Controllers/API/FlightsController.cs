using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AIS.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRecordRepository _ticketRecordRepository;
        private readonly IUserHelper _userHelper;

        public FlightsController(IFlightRepository flightRepository, ITicketRecordRepository ticketRecordRepository, IUserHelper userHelper)
        {
            _flightRepository = flightRepository;
            _ticketRecordRepository = ticketRecordRepository;
            _userHelper = userHelper;
        }

        // GET api all scheduled client flights
        [HttpGet]
        public async Task<IActionResult> GetCLientFutureFlights()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token!");
            }

            User user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                return Unauthorized("User not found!");
            }

            List<Flight> flights = await _flightRepository.GetFlightsTrackIncludeAsync();

            return Ok(flights.Where(f => f.User == user && f.Departure > DateTime.UtcNow));
        }

        // GET api client flight (ticket) records
        [HttpGet]
        public async Task<IActionResult> GetCLientFlightRecords()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token!");
            }

            User user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                return Unauthorized("User not found!");
            }

            return Ok(_ticketRecordRepository.GetAll().Where(t => t.UserId == userId));
        }

        // GET api all Flights
        [HttpGet]
        public async Task<IActionResult> GetAllFlights()
        {
            return Ok(await _flightRepository.GetFlightsTrackIncludeAsync());
        }

        // GET api/<FlightsController>/5 by flight Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlightById(int id)
        {
            return Ok(await _flightRepository.GetFlightTrackIncludeByIdAsync(id));
        }
    }
}
