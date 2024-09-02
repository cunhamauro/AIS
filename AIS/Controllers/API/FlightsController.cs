using AIS.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public FlightsController(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        // GET api All Flights
        [HttpGet]
        public async Task<IActionResult> GetAllFlights()
        {
            return Ok(await _flightRepository.GetFlightsIncludeAsync());
        }

        // GET api/<FlightsController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlightById(int id)
        {
            return Ok(await _flightRepository.GetFlightIncludeByIdAsync(id));
        }
    }
}
