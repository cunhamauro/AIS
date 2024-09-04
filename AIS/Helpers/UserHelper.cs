using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AIS.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAirportRepository _airportRepository;
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IFlightRepository _flightRepository;

        public UserHelper(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, IAirportRepository airportRepository, IAircraftRepository aircraftRepository, IFlightRepository flightRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _airportRepository = airportRepository;
            _aircraftRepository = aircraftRepository;
            _flightRepository = flightRepository;
        }

        /// <summary>
        /// Check if an User is registered in any of the Entities
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>User in Entities?</returns>
        public async Task<bool> UserInEntities(User user)
        {
            bool userInAircrafts = await _aircraftRepository.GetAll().Include(a => a.User).Where(a => a.User.Id == user.Id).AnyAsync();
            bool userInAirports = await _airportRepository.GetAll().Include(a => a.User).Where(a => a.User.Id == user.Id).AnyAsync();
            bool userInFlights = await _flightRepository.GetAll().Include(a => a.User).Where(a => a.User.Id == user.Id).AnyAsync();

            if (userInAircrafts || userInAirports || userInFlights)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a user from all entity records and registers an admin
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Task</returns>
        public async Task RemoveUserFromEntities(User user, ClaimsPrincipal adminClaims)
        {
            // Get the list of entities with the user registered
            var userAircrafts = await _aircraftRepository.GetAll().Include(a => a.User).Where(a => a.User.Id == user.Id).ToListAsync();
            var userAirports = await _airportRepository.GetAll().Include(a => a.User).Where(a => a.User.Id == user.Id).ToListAsync();
            var userFlights = await _flightRepository.GetAll().Include(a => a.User).Where(a => a.User.Id == user.Id).ToListAsync();

            var admin = await _userManager.GetUserAsync(adminClaims); // Get the admin that is removing the user

            if (userAircrafts.Any())
            {
                await _aircraftRepository.AircraftsFromUserToAdmin(userAircrafts, admin);
            }

            if (userAirports.Any())
            {
                await _airportRepository.AirportsFromUserToAdmin(userAirports, admin);
            }

            if (userFlights.Any())
            {
                await _flightRepository.FlightsFromUserToAdmin(userFlights, admin);
            }
        }

        /// <summary>
        /// Deletes the user
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Task</returns>
        public async Task DeleteUserAsync(User user)
        {
            await _userManager.DeleteAsync(user);
        }

        /// <summary>
        /// Updates the user email & username & the normalized versions
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="newEmail">New email</param>
        /// <returns>Task</returns>
        public async Task UpdateUserEmailAndUsernameAsync(User user, string newEmail)
        {
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            await _userManager.ChangeEmailAsync(user, newEmail, token);
            await _userManager.SetUserNameAsync(user, newEmail);
            await _userManager.UpdateNormalizedEmailAsync(user);
            await _userManager.UpdateNormalizedUserNameAsync(user);
        }

        /// <summary>
        /// Get all users with their roles
        /// </summary>
        /// <returns>List of users with their roles</returns>
        public async Task<List<UserWithRolesViewModel>> GetUsersIncludeRolesAsync()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var usersWithRoles = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRolesViewModel
                {
                    User = user,
                    Roles = roles
                });
            }

            return usersWithRoles;
        }

        /// <summary>
        /// Get an User by User ID including their assigned roles
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User with roles</returns>
        public async Task<UserWithRolesViewModel> GetUserByIdIncludeRoleAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var userWithRoles = new UserWithRolesViewModel
            {
                User = user,
                Roles = await _userManager.GetRolesAsync(user),

            };

            return userWithRoles;
        }

        /// <summary>
        /// Get the profile image of an User
        /// </summary>
        /// <param name="userClaims">Claims of User</param>
        /// <returns>Profile image</returns>
        public async Task<string> GetUserProfileImageAsync(ClaimsPrincipal userClaims)
        {
            var userId = _userManager.GetUserId(userClaims);
            var user = await _userManager.FindByIdAsync(userId);
            return user.ImageDisplay;
        }

        public async Task<User> GetUserAsync(ClaimsPrincipal userClaims)
        {
            return await _userManager.GetUserAsync(userClaims);
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                true);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName,
                });
            }
        }

        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task RemoveUserFromRoleAsync(User user, string roleName)
        {
            await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<SignInResult> ValidatePasswordAsync(User user, string password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, true);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }
    }
}
