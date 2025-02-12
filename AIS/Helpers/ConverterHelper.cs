﻿using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

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

        public User ToUser(CreateUserViewModel model)
        {
            return new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                UserName = model.Email,
            };
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
                IsActive = model.IsActive,
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
                IsActive = aircraft.IsActive,
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
                OriginId = flight.Origin.Id,
                DestinationId = flight.Destination.Id,
            };
        }

        public TicketViewModel ToTicketViewModel(Ticket ticket, Flight flight, List<SelectListItem> listSeats)
        {
            return new TicketViewModel
            {
                FlightNumber = flight.FlightNumber,
                OriginCityCountry = $"{flight.Origin.City}, {flight.Origin.Country}",
                DestinationCityCountry = $"{flight.Destination.City}, {flight.Destination.Country}",
                DepartureDate = flight.Departure,
                ArrivalDate = flight.Arrival,
                Id = ticket.Id,
                SeatsList = listSeats,
                FullName = ticket.FullName,
                Email = ticket.Email,
                IdNumber = ticket.IdNumber,
                PhoneNumber = ticket.PhoneNumber,
                DateOfBirth = ticket.DateOfBirth,
                Seat = ticket.Seat,
                Title = ticket.Title,
                Flight = flight,
                FlightId = flight.Id,
                Price = ticket.Price,

            };
        }
    }
}
