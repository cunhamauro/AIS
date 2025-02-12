﻿using AIS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Data.Repositories
{
    public class FlightRepository : GenericRepository<Flight>, IFlightRepository
    {
        private readonly DataContext _context;

        public FlightRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Clean all flights (and its tickets) that have already departed
        /// </summary>
        /// <returns>Task</returns>
        public async Task DeleteOldFlightsAsync()
        {
            List<Flight> oldFlights = await _context.Flights.Include(t => t.TicketList).Where(f => f.Departure < DateTime.UtcNow).ToListAsync();
            List<Ticket> oldTickets = new List<Ticket>();

            foreach (var flight in oldFlights)
            {
                if (flight.TicketList.Any())
                {
                    oldTickets.AddRange(flight.TicketList);
                }
            }

            if (oldTickets.Any())
            {
                _context.Tickets.RemoveRange(oldTickets);
            }

            if (oldFlights.Any())
            {
                _context.Flights.RemoveRange(oldFlights);
            }

            if (oldFlights.Any() || oldTickets.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get a Flight by ID including nested Entities
        /// </summary>
        /// <param name="id">Flight ID</param>
        /// <returns>Flight</returns>
        public async Task<Flight> GetFlightTrackIncludeByIdAsync(int id)
        {
            return await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Origin)
                .Include(f => f.Destination)
                .Include(f => f.TicketList)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        /// <summary>
        /// Get all Flights including nested Entities
        /// </summary>
        /// <returns>List of Flights</returns>
        public async Task<List<Flight>> GetFlightsTrackIncludeAsync()
        {
            return await _context.Flights
            .Include(f => f.Aircraft)
            .Include(f => f.Origin)
            .Include(f => f.Destination).ToListAsync();
        }

        /// <summary>
        /// Check if an Airport is part of a Flight
        /// </summary>
        /// <param name="id">Airport ID</param>
        /// <returns>Airport part of Flight?</returns>
        public async Task<bool> AirportInFlights(int id)
        {
            return await _context.Flights.AnyAsync(a => a.Origin.Id == id || a.Destination.Id == id);
        }

        /// <summary>
        /// Check if an Aircraft is part of a Flight
        /// </summary>
        /// <param name="id">Aircraft ID</param>s
        /// <returns>Aircraft part of Flight?</returns>
        public async Task<bool> AircraftInFlights(int id)
        {
            return await _context.Flights.AnyAsync(a => a.Aircraft.Id == id);
        }

        /// <summary>
        /// Transfer 'ownership' of all Flights from an User to the current Admin
        /// </summary>
        /// <param name="userFlights">User</param>
        /// <param name="admin">Admin</param>
        /// <returns>Task</returns>
        public async Task FlightsFromUserToAdmin(List<Flight> userFlights, User admin)
        {
            foreach (Flight flight in userFlights)
            {
                flight.User = admin;
            }

            await _context.SaveChangesAsync();
        }
    }
}
