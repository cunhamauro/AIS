using AIS.Data.Entities;
using AIS.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Data
{
    public class SeedDatabase
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;

        public SeedDatabase(DataContext context, IUserHelper userHelper, IConfiguration configuration)
        {
            _context = context;
            _userHelper = userHelper;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
 
            // Remove flights that have a departure time + 1 hour < current time
            //var currentTime = DateTime.UtcNow.AddHours(1);
            //var flightsToDelete = _context.Flights.AsNoTracking().Where(f => f.Departure < currentTime);
            //_context.Flights.RemoveRange(flightsToDelete);
            //await _context.SaveChangesAsync();

            await _userHelper.CheckRoleAsync(_configuration["Admin:Role"]);
            await _userHelper.CheckRoleAsync("Client");

            var user = await _userHelper.GetUserByEmailAsync(_configuration["Admin:Email"]);

            if (user == null)
            {
                user = new User
                {
                    FirstName = _configuration["Admin:FirstName"],
                    LastName = _configuration["Admin:LastName"],
                    Email = _configuration["Admin:Email"],
                    UserName = _configuration["Admin:UserName"],
                    PhoneNumber = _configuration["Admin:PhoneNumber"],
                };

                var result = await _userHelper.AddUserAsync(user, _configuration["Admin:Password"]);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Could not create the user {_configuration["Admin:Role"]} in seeder!");
                }

                await _userHelper.AddUserToRoleAsync(user, _configuration["Admin:Role"]);

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);
            }

            var isInRole = await _userHelper.IsUserInRoleAsync(user, _configuration["Admin:Role"]);

            if (!isInRole)
            {
                await _userHelper.AddUserToRoleAsync(user, _configuration["Admin:Role"]);
            }
        }
    }
}
