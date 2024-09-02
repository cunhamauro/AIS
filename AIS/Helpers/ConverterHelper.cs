using AIS.Data;
using AIS.Data.Entities;
using AIS.Models;
using System.Threading.Tasks;

namespace AIS.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IAirportRepository _airportRepository;

        public ConverterHelper(IAircraftRepository aircraftRepository, IAirportRepository airportRepository)
        {
            _aircraftRepository = aircraftRepository;
            _airportRepository = airportRepository;
        }

        public Aircraft ToAircraft(AircraftViewModel model, string path, bool isNew)
        {
            return new Aircraft
            {
                Id = isNew ? 0 : model.Id,
                ImageUrl = path,
                Capacity = model.Capacity,
                Rows = model.Rows,
                Model = model.Model,
                Seats = model.Seats,
                Status = model.Status,
                User = model.User,
            };
        }

        public AircraftViewModel ToAircraftViewModel(Aircraft aircraft)
        {
            return new AircraftViewModel
            {
                Id = aircraft.Id,
                ImageUrl = aircraft.ImageUrl,
                Capacity = aircraft.Capacity,
                Rows = aircraft.Rows,
                Model = aircraft.Model,
                Seats = aircraft.Seats,
                Status = aircraft.Status,
                User = aircraft.User,
            };
        }

        public Airport ToAirport(string iata, string country, string city)
        {
            return new Airport
            {
                Id = 0,
                IATA = iata,
                Country = country,
                City = city,
            };
        }

        public Flight ToFlight(Aircraft aircraft, Airport origin, Airport destination, FlightViewModel model, bool isNew)
        {
            return new Flight
            {
                Id = isNew ? 0 : model.Id,
                Aircraft = aircraft,
                Origin = origin,
                Destination = destination,
                Departure = model.Departure,
                Arrival = model.Arrival,
                User = model.User,
            };
        }

        public FlightViewModel ToFlightViewModel(Flight flight)
        {
            return new FlightViewModel
            {
                Id = flight.Id,
                AircraftId = flight.Aircraft.Id,
                Departure = flight.Departure,
                Arrival = flight.Arrival,
                FlightNumber = flight.FlightNumber,
                Origin = flight.Origin,
                Destination = flight.Destination,
            };
        }
    }
}
