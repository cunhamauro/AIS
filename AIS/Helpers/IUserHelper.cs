using AIS.Data.Entities;
using AIS.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AIS.Helpers
{
    public interface IUserHelper
    {
        Task<bool> UserInEntities(User user);

        Task RemoveUserFromEntities(User user, ClaimsPrincipal admin);

        Task DeleteUserAsync(User user);

        Task UpdateUserEmailAndUsernameAsync(User user, string newEmail);

        Task<List<UserWithRolesViewModel>> GetUsersIncludeRolesAsync();

        Task<UserWithRolesViewModel> GetUserByIdIncludeRoleAsync(string userId);

        Task<string> GetUserProfileImageAsync(ClaimsPrincipal userClaims);

        Task<User> GetUserAsync(ClaimsPrincipal principal);

        Task<User> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task LogoutAsync();

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleAsync(User user, string roleName);

        Task RemoveUserFromRoleAsync(User user, string roleName);

        Task<bool> IsUserInRoleAsync(User user, string roleName);

        Task<SignInResult> ValidatePasswordAsync(User user, string password);

        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        Task<IdentityResult> ConfirmEmailAsync(User user, string token);

        Task<User> GetUserByIdAsync(string userId);
    }
}
