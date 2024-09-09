using AIS.Data.Entities;
using AIS.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AIS.Data
{
    public class SeedDatabase
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;

        public SeedDatabase(DataContext context, IUserHelper userHelper, IConfiguration configuration, UserManager<User> userManager)
        {
            _context = context;
            _userHelper = userHelper;
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Client");
            await _userHelper.CheckRoleAsync("Employee");

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
                    throw new InvalidOperationException($"Could not create the user Admin in seeder!");
                }

                await _userHelper.AddUserToRoleAsync(user, "Admin");

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);
            }

            var isInRole = await _userHelper.IsUserInRoleAsync(user, "Admin");

            if (!isInRole)
            {
                await _userHelper.AddUserToRoleAsync(user, "Admin");
            }
        }
    }
}
