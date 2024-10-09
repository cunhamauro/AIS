using AIS.Data.Classes;
using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Helpers;
using AIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Syncfusion.EJ2.Notifications;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AIS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;
        private readonly IMailHelper _mailHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IAirportRepository _airportRepository;
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IConverterHelper _converterHelper;

        public AccountController(IUserHelper userHelper, IConfiguration configuration, IMailHelper mailHelper, IImageHelper imageHelper, IAirportRepository airportRepository, IAircraftRepository aircraftRepository, IFlightRepository flightRepository, ITicketRepository ticketRepository, IConverterHelper converterHelper)
        {
            _userHelper = userHelper;
            _configuration = configuration;
            _mailHelper = mailHelper;
            _imageHelper = imageHelper;
            _airportRepository = airportRepository;
            _aircraftRepository = aircraftRepository;
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _converterHelper = converterHelper;
        }

        // GET: Users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var listUsersIncludeRole = await _userHelper.GetUsersIncludeRolesAsync();

            return View(listUsersIncludeRole);
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> DeleteAccount()
        {
            User user = await _userHelper.GetUserAsync(this.User);

            if (await _ticketRepository.ClientHasTickets(user.Id))
            {
                ViewBag.ShowMsg = true;
                ViewBag.State = "disabled";
            }

            return View(new DeleteAccountViewModel());
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> DeleteAccount(DeleteAccountViewModel model)
        {
            User user = await _userHelper.GetUserAsync(this.User);

            if (await _ticketRepository.ClientHasTickets(user.Id))
            {
                return View();
            }

            var validation = await _userHelper.ValidatePasswordAsync(user, model.Password);

            if (!validation.Succeeded)
            {
                ModelState.AddModelError("Password", "Incorrect Password!");
            }

            if (ModelState.IsValid)
            {
                await _userHelper.DeleteUserAsync(user);
                await _userHelper.LogoutAsync();

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // GET: Account/Delete/userid
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            User userCheck = await _userHelper.GetUserByIdAsync(id);
            if (string.IsNullOrEmpty(id) || userCheck == null)
            {
                return UserNotFound();
            }

            UserWithRolesViewModel userModel = await _userHelper.GetUserByIdIncludeRoleAsync(id);
            User user = userModel.User;
            User currentAdmin = await _userHelper.GetUserAsync(this.User);

            if (await _ticketRepository.ClientHasTickets(userCheck.Id))
            {
                ViewBag.ShowMsg = true;
                ViewBag.Message = "Account deletion not allowed! This user has tickets for scheduled flights.";
                ViewBag.State = "disabled";

                return View(userModel);
            }

            if (user.Id == currentAdmin.Id) // Dont allow the current account to delete itself
            {
                ViewBag.ShowMsg = true;
                ViewBag.Message = "You are not allowed to delete yourself!";
                ViewBag.State = "disabled";

                return View(userModel);
            }

            User userMasterAdmin = await _userHelper.GetUserByEmailAsync(_configuration["Admin:Email"]);

            if (user.Id == userMasterAdmin.Id) // Or to delete the master admin
            {
                ViewBag.ShowMsg = true;
                ViewBag.Message = "You are not allowed to delete the Master Admin!";
                ViewBag.State = "disabled";

                return View(userModel);
            }

            ViewBag.ShowMsg = await _userHelper.UserInEntities(await _userHelper.GetUserByIdAsync(id));
            ViewBag.Message = "This User is registered in current entities!";

            return View(userModel);
        }

        // POST: Account/Delete/userid
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            User user = await _userHelper.GetUserByIdAsync(id);

            if (await _ticketRepository.ClientHasTickets(id))
            {
                return View();
            }

            User currentAdmin = await _userHelper.GetUserAsync(this.User);

            if (user.Id == currentAdmin.Id)
            {
                return RedirectToAction("Delete", "Account");
            }

            if (await _userHelper.UserInEntities(user)) // If User is registered in Entities
            {
                await _userHelper.RemoveUserFromEntities(user, this.User); // Remove him from those Entities and assign them the current Admin
            }

            // Delete profile image when User is also deleted
            if (!string.IsNullOrEmpty(user.ImageUrl) && user.ImageUrl != @"~/images/default-profile-image.png") // Dont delete if its the no image
            {
                _imageHelper.DeleteImage(user.ImageUrl);
            }

            await _userHelper.DeleteUserAsync(user); // Finally, delete the user

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);

                if (result.Succeeded)
                {
                    if (this.Request.Query.Keys.Contains("ReturnUrl")) // ReturnUrl -> Sends the user to the page he was previously of logging in
                    {
                        var returnUrl = this.Request.Query["ReturnUrl"].First();

                        if (!returnUrl.Contains("Login") && !returnUrl.Contains("Register")) // If the previous page is neither login or register
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ViewBag.MsgId = "msg_error";
            ViewBag.Severity = Severity.Error;
            ViewBag.LoginMessage = "Login failed!";

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);

                if (user == null)
                {
                    user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Username,
                        UserName = model.Username,
                        PhoneNumber = model.PhoneNumber,
                    };

                    var result = await _userHelper.AddUserAsync(user, model.Password);

                    if (result != IdentityResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, "User registration failed!");
                        return View(model);
                    }

                    await _userHelper.AddUserToRoleAsync(user, "Client");

                    string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    string tokenLink = Url.Action("EmailConfirmation", "Account", new
                    {
                        userid = user.Id,
                        token = myToken,
                    }, protocol: HttpContext.Request.Scheme);

                    string emailBody = _mailHelper.GetHtmlTemplateTokenLink("Complete your account activation", "confirm your email", "Confirm", tokenLink);

                    Response response = await _mailHelper.SendEmailAsync(model.Username, "Email Confirmation!", emailBody, null, null, null);

                    if (response.IsSuccess)
                    {
                        return DisplayMessage("Email Confirmation", "The instructions to activate your account have been sent to your email!");
                    }
                    else
                    {
                        return DisplayMessage("Email not sent", "Did he miss his flight?");
                    }
                }
                else
                {
                    ViewBag.MsgId = "msg_error";
                    ViewBag.Severity = Severity.Error;
                    ViewBag.RegisterMessage = "Email already registered!";
                }
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> ChangeUser()
        {
            var user = await _userHelper.GetUserAsync(User);
            var model = new ChangeUserViewModel();

            if (user != null)
            {
                model.ImageUrl = user.ImageUrl;
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.PhoneNumber = user.PhoneNumber;
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool noChanges = false;

                // Update the image
                var path = model.ImageUrl;
                string oldPath = path;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    path = await _imageHelper.UploadImageAsync(model.ImageFile, model.LastName, "users");
                }

                var user = await _userHelper.GetUserAsync(User);

                if (user != null)
                {
                    if (user.FirstName == model.FirstName && user.LastName == model.LastName && user.PhoneNumber == model.PhoneNumber)
                    {
                        noChanges = true;
                    }

                    user.ImageUrl = path;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;

                    var result = await _userHelper.UpdateUserAsync(user);

                    if (path != oldPath && oldPath != @"~/images/default-profile-image.png") // Dont delete if its the default image
                    {
                        // Delete old image when it is different from the new one (updated)
                        if (!string.IsNullOrEmpty(oldPath))
                        {
                            _imageHelper.DeleteImage(oldPath);
                        }

                        noChanges = false;
                    }

                    if (noChanges)
                    {
                        ViewBag.Severity = Severity.Normal;
                        ViewBag.MsgId = "msg_default";
                        ViewBag.UserMessage = "No changes to user data!";
                    }
                    else if (result.Succeeded)
                    {
                        ViewBag.Severity = Severity.Success;
                        ViewBag.MsgId = "msg_success";
                        ViewBag.UserMessage = "User data updated!";
                    }
                    else
                    {
                        ViewBag.Severity = Severity.Error;
                        ViewBag.MsgId = "msg_error";
                        ViewBag.UserMessage = "User data update failed!";
                    }
                }
            }

            return View(model);
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserAsync(User);

                if (user != null)
                {
                    if (model.CurrentPassword == model.NewPassword)
                    {
                        ViewBag.Severity = Severity.Normal;
                        ViewBag.MsgId = "msg_default";
                        ViewBag.UserMessage = "Current password and new password are the same!";

                        return View(model);
                    }

                    var result = await _userHelper.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                    if (result.Succeeded)
                    {
                        ViewBag.Severity = Severity.Success;
                        ViewBag.MsgId = "msg_success";
                        ViewBag.UserMessage = "User password updated!";
                    }
                    else
                    {
                        ViewBag.Severity = Severity.Error;
                        ViewBag.MsgId = "msg_error";
                        ViewBag.UserMessage = "User password update failed!";
                    }
                }
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);

                if (user != null)
                {
                    var result = await _userHelper.ValidatePasswordAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            _configuration["Tokens:Issuer"],
                            _configuration["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(7),
                            signingCredentials: credentials);

                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };

                        return this.Created(string.Empty, results);
                    }
                }
            }
            return BadRequest();
        }

        // GET: Account/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            CreateUserViewModel model = new CreateUserViewModel();

            return View(model);
        }

        // POST: Account/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (model.RoleName != "Admin" && model.RoleName != "Client" && model.RoleName != "Employee")
            {
                ModelState.AddModelError("RoleName", "Select a valid Role!");
            }

            if (ModelState.IsValid)
            {
                User userExists = await _userHelper.GetUserByEmailAsync(model.Email);

                if (userExists == null)
                {
                    User user = _converterHelper.ToUser(model);

                    string password = "";
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    var random = new Random();

                    for (int i = 0; i < 6; i++)
                    {
                        password += chars[random.Next(chars.Length)];
                    }

                    var result = await _userHelper.AddUserAsync(user, password);

                    if (result != IdentityResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, "User registration failed!");
                        return View(model);
                    }

                    await _userHelper.AddUserToRoleAsync(user, model.RoleName);

                    // Automatically confirm the email
                    string userToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    await _userHelper.ConfirmEmailAsync(user, userToken);

                    // Start password reset proccess so the user can set his own password through his email
                    string passwordResetToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                    string tokenLink = Url.Action("PasswordReset", "Account", new
                    {
                        userid = user.Id,
                        token = passwordResetToken,
                    }, protocol: HttpContext.Request.Scheme);

                    string emailBody = _mailHelper.GetHtmlTemplateTokenLink("Configure your account password", "set your password", "Password", tokenLink);

                    Response response = await _mailHelper.SendEmailAsync(model.Email, "Password Configuration!", emailBody, null, null, null);

                    if (!response.IsSuccess)
                    {
                        return DisplayMessage("Email not sent", "Did he miss his flight?");
                    }

                    return RedirectToAction(nameof(Index));
                }

                ViewBag.MsgId = "msg_error";
                ViewBag.Severity = Severity.Error;
                ViewBag.RegisterMessage = "There is already an account with this Email!";
            }
            return View(new CreateUserViewModel { FirstName = model.FirstName, LastName = model.LastName, PhoneNumber = model.PhoneNumber });
        }

        // GET: Account/Edit/userid
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return UserNotFound();
            }

            User user = await _userHelper.GetUserByIdAsync(id);
            EditUserViewModel model = new EditUserViewModel
            {
                Email = user.Email,
                UserId = user.Id,
            };

            if (model == null)
            {
                return UserNotFound();
            }

            if (model.Email == _configuration["Admin:Email"])
            {
                ViewBag.Message = "The Master Admin's email cannot be changed!";

                return View(model);
            }

            return View(model);
        }

        // GET: Account/Edit/userid
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = await _userHelper.GetUserByIdAsync(model.UserId);

            if (user.Email != _configuration["Admin:Email"])
            {
                await _userHelper.UpdateUserEmailAndUsernameAsync(user, model.Email);
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Edit", "Account");
        }

        // GET: Account/Details/userId
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return UserNotFound();
            }

            UserWithRolesViewModel userModel = await _userHelper.GetUserByIdIncludeRoleAsync(id);

            if (userModel == null)
            {
                return UserNotFound();
            }
            return View(userModel);
        }

        public IActionResult ForgotPassword()
        {
            // Ensure no active user session is affecting this operation
            if (User.Identity.IsAuthenticated)
            {
                return NotAuthorized();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            // Ensure no active user session is affecting this operation
            if (User.Identity.IsAuthenticated)
            {
                return NotAuthorized();
            }

            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserByEmailAsync(model.Email);

                if (user != null)
                {
                    // Start password reset proccess so the user can recover his password trough his email
                    string passwordResetToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                    string tokenLink = Url.Action("PasswordReset", "Account", new
                    {
                        userid = user.Id,
                        token = passwordResetToken,
                    }, protocol: HttpContext.Request.Scheme);

                    string emailBody = _mailHelper.GetHtmlTemplateTokenLink("Recover your account password", "recover your password", "Password", tokenLink);

                    Response response = await _mailHelper.SendEmailAsync(model.Email, "Password Recovery!", emailBody, null, null, null);

                    if (!response.IsSuccess)
                    {
                        return DisplayMessage("Email not sent", "Did he miss his flight?");
                    }
                    return DisplayMessage("Password Recovery", "The instructions to recover your password have been sent to your email!");
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult PasswordReset(string userId, string token)
        {
            // Ensure no active user session is affecting this operation
            if (User.Identity.IsAuthenticated)
            {
                return NotAuthorized();
            }

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return UserNotFound();
            }

            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordReset(ResetPasswordViewModel model)
        {
            // Ensure no active user session is affecting this operation
            if (User.Identity.IsAuthenticated)
            {
                return NotAuthorized();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper.GetUserByIdAsync(model.UserId);

            if (user == null)
            {
                return UserNotFound();
            }

            var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                return DisplayMessage("Password Set", "Your account password has been successfully set. You can now log in!");
            }

            return DisplayMessage("Password configuration failed", "Your password configuration has failed! Try again.");
        }

        public async Task<IActionResult> EmailConfirmation(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return UserNotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                return UserNotFound();
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return DisplayMessage("Email confirmation failure", "Your account activation has failed! Try again.");
            }
            return DisplayMessage("Email confirmed", "Your account activation was succesfully completed! You can now log in.");
        }

        public IActionResult UserNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "User not found", Message = "Maybe he jumped from the plane?" });
        }

        public IActionResult NotAuthorized()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Not authorized", Message = $"Restricted airspace!" });
        }

        public IActionResult DisplayMessage(string title, string message)
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = $"{title}", Message = $"{message}" });
        }
    }
}
