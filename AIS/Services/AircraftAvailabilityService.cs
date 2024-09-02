using AIS.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AIS.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AIS.Services
{
    public class AircraftAvailabilityService : IAircraftAvailabilityService
    {
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IFlightRepository _flightRepository;

        public AircraftAvailabilityService(IAircraftRepository aircraftRepository, IFlightRepository flightRepository)
        {
            _aircraftRepository = aircraftRepository;
            _flightRepository = flightRepository;
        }

        /// <summary>
        /// Get a list of available Aircrafts
        /// </summary>
        /// <param name="departure">Departure date and hours</param>
        /// <param name="arrival">Arrival date and hours</param>
        /// <param name="origin">Airport of origin</param>
        /// <returns>List of available Aircrafts</returns>
        public async Task<List<Aircraft>> AvailableAircrafts(DateTime departure, DateTime arrival, Airport origin)
        {
            List<Aircraft> _listAircrafts = await _aircraftRepository.GetAll().ToListAsync();

            List<Aircraft> availableAircrafts = new List<Aircraft>();

            foreach (Aircraft aircraft in _listAircrafts)
            {
                if (await AircraftAvailableOnDate(aircraft, departure, arrival, origin)) // Verify if the aircraft is available on the given date
                {
                    availableAircrafts.Add(aircraft);
                }
            }

            return availableAircrafts;
        }

        /// <summary>
        /// Check an Aircraft availability for a given date and flight airport sequence
        /// </summary>
        /// <param name="aircraft">Aircraft to check</param>
        /// <param name="checkDateDeparture">Date of departure</param>
        /// <param name="checkDateArrival">Date of arrival</param>
        /// <param name="origin">Airport of origin</param>
        /// <returns></returns>
        public async Task<bool> AircraftAvailableOnDate(Aircraft aircraft, DateTime checkDateDeparture, DateTime checkDateArrival, Airport origin)
        {
            List<Flight> listFlights = await _flightRepository.GetFlightsIncludeAsync();

            // Find the latest flight before the departure date
            Flight previousFlight = listFlights.Where(f => f.Aircraft.Id == aircraft.Id && f.Arrival < checkDateDeparture).OrderByDescending(f => f.Arrival).FirstOrDefault();

            // Find the earliest next flight after the departure date
            Flight nextFlight = listFlights.Where(f => f.Aircraft.Id == aircraft.Id && f.Departure > checkDateArrival).OrderBy(f => f.Departure).FirstOrDefault();

            if (previousFlight != null) // If a previous flight exists
            {
                // Check if the aircraft is at the destination of the previous flight and the dates match
                if (previousFlight.Destination.Id != origin.Id)
                {
                    return false; // The aircraft is not at the required origin airport
                }
                else
                {
                    // If the departure of the new flight is before the adjusted arrival time, the aircraft is not available
                    if (checkDateDeparture < previousFlight.Arrival.AddHours(1))
                    {
                        return false;
                    }
                }
            }

            if (nextFlight != null) // If a scheduled next flight exists
            {
                // If the arrival of the new flight is after the adjusted departure time of the next flight, the aircraft is not available
                if (checkDateArrival > nextFlight.Departure.AddHours(-1))
                {
                    return false;
                }
            }

            // Check for any overlapping flights
            foreach (var flight in listFlights)
            {
                if (aircraft.Id == flight.Aircraft.Id)
                {
                    if ((checkDateDeparture >= flight.Departure.AddHours(-1) && checkDateDeparture <= flight.Arrival.AddHours(1)) ||
                        (checkDateArrival >= flight.Departure.AddHours(-1) && checkDateArrival <= flight.Arrival.AddHours(1)) ||
                        (checkDateDeparture <= flight.Departure.AddHours(-1) && checkDateArrival >= flight.Arrival.AddMinutes(1)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
