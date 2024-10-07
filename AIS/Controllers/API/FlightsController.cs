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
        private readonly IFlightRecordRepository _flightRecordRepository;

        public FlightsController(IFlightRepository flightRepository, ITicketRecordRepository ticketRecordRepository, IUserHelper userHelper, IFlightRecordRepository flightRecordRepository)
        {
            _flightRepository = flightRepository;
            _ticketRecordRepository = ticketRecordRepository;
            _userHelper = userHelper;
            _flightRecordRepository = flightRecordRepository;
        }

        // GET api all scheduled client flights
        [HttpGet]
        public async Task<IActionResult> GetClientFutureFlights()
        {
            var username = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            User user = await _userHelper.GetUserByEmailAsync(username);

            if (string.IsNullOrEmpty(username) || user == null)
            {
                return Unauthorized("User not found!");
            }

            List<TicketRecord> ticketRecords = await _ticketRecordRepository.GetAll().Where(f => f.Departure > DateTime.UtcNow).ToListAsync();

            return Ok(ticketRecords);
        }

        // GET api/<FlightsController>/5 by flight Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlightById(int id)
        {
            FlightRecord flight = await _flightRecordRepository.GetByIdAsync(id);
            return Ok(flight);
        }
    }
}
