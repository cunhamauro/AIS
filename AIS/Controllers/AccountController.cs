using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Helpers;
using AIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Syncfusion.EJ2.Notifications;
using System;
using System.Diagnostics;
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

        public AccountController(IUserHelper userHelper, IConfiguration configuration, IMailHelper mailHelper, IImageHelper imageHelper, IAirportRepository airportRepository, IAircraftRepository aircraftRepository, IFlightRepository flightRepository)
        {
            _userHelper = userHelper;
            _configuration = configuration;
            _mailHelper = mailHelper;
            _imageHelper = imageHelper;
            _airportRepository = airportRepository;
            _aircraftRepository = aircraftRepository;
            _flightRepository = flightRepository;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var listUsersIncludeRole = await _userHelper.GetUsersIncludeRolesAsync();

            return View(listUsersIncludeRole);
        }

        // GET: Account/Edit/userid
        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return UserNotFound();
            }

            UserWithRolesViewModel userModel = await _userHelper.GetUserByIdIncludeRoleAsync(id);
            ChangeRolesEmailViewModel rolesModel = new ChangeRolesEmailViewModel
            {
                UserWithRoles = userModel,
                Email = userModel.User.Email,
            };

            rolesModel.GetRoles();

            if (rolesModel == null)
            {
                return UserNotFound();
            }

            if (userModel.User.Email == _configuration["Admin:Email"])
            {
                ViewBag.ShowMsg = true;
                ViewBag.Message = "The Master Admin can not lose the Admin role!";
                ViewBag.State = true; // Disable the Admin checkbox so it cannot be changed

                return View(rolesModel);
            }

            return View(rolesModel);
        }

        // GET: Account/Edit/userid
        [HttpPost]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(ChangeRolesEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool client = model.IsClient;

            var user = await _userHelper.GetUserByIdAsync(model.UserWithRoles.User.Id);

            if (model.IsAdmin && !await _userHelper.IsUserInRoleAsync(user, "Admin"))
            {
                await _userHelper.AddUserToRoleAsync(user, "Admin");
            }
            else if (!model.IsAdmin && await _userHelper.IsUserInRoleAsync(user, "Admin"))
            {
                if (user.Email != _configuration["Admin:Email"])
                {
                    await _userHelper.RemoveUserFromRoleAsync(user, "Admin");
                }
            }

            if (model.IsEmployee && !await _userHelper.IsUserInRoleAsync(user, "Employee"))
            {
                await _userHelper.AddUserToRoleAsync(user, "Employee");
            }
            else if (!model.IsEmployee && await _userHelper.IsUserInRoleAsync(user, "Employee"))
            {
                await _userHelper.RemoveUserFromRoleAsync(user, "Employee");
            }

            if (model.IsClient && !await _userHelper.IsUserInRoleAsync(user, "Client"))
            {
                await _userHelper.AddUserToRoleAsync(user, "Client");
            }
            else if (!model.IsClient && await _userHelper.IsUserInRoleAsync(user, "Client"))
            {
                await _userHelper.RemoveUserFromRoleAsync(user, "Client");
            }

            await _userHelper.UpdateUserEmailAndUsernameAsync(user, model.Email);

            return RedirectToAction(nameof(Index));
        }



        // GET: Account/Details/userId
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

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // GET: Account/Delete/userid
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return UserNotFound();
            }

            UserWithRolesViewModel userModel = await _userHelper.GetUserByIdIncludeRoleAsync(id);
            User user = userModel.User;
            User currentAdmin = await _userHelper.GetUserAsync(this.User);

            if (user.Id == currentAdmin.Id) // Dont allow the current account to delete itself
            {
                ViewBag.ShowMsg = true;
                ViewBag.Message = "An User (Admin) is not allowed to delete itself!";
                ViewBag.State = "disabled";

                return View(userModel);
            }

            ViewBag.ShowMsg = await _userHelper.UserInEntities(await _userHelper.GetUserByIdAsync(id));
            ViewBag.Message = "This User is registered in current entities!";

            if (userModel == null)
            {
                return UserNotFound();
            }

            return View(userModel);
        }

        // POST: Aircrafts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            User user = await _userHelper.GetUserByIdAsync(id);

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

                    await _userHelper.AddUserToRoleAsync(user, "Client");

                    if (result != IdentityResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, "User registration failed!");
                        return View(model);
                    }

                    string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    string tokenLink = Url.Action("EmailConfirmation", "Account", new
                    {
                        userid = user.Id,
                        token = myToken,
                    }, protocol: HttpContext.Request.Scheme);

                    #region Email Body

                    string emailBody = $@"<body class=""body"" style=""background-color:#005a9c;margin:0;padding:0;-webkit-text-size-adjust:none;text-size-adjust:none"">
                        <table class=""nl-container"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0;background-color:#005a9c"">
                            <tbody>
                                <tr>
                                    <td>
                                        <table class=""row row-2"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0"">
                                            <tbody>
                                                <tr>
                                                    <td>
                                                        <table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0;background-color:#fff;color:#000;width:510px;margin:0 auto"" width=""510"">
                                                            <tbody>
                                                                <tr>
                                                                    <td class=""column column-1"" width=""100%"" style=""mso-table-lspace:0;mso-table-rspace:0;font-weight:400;text-align:left;padding-bottom:30px;padding-left:30px;padding-right:30px;padding-top:30px;vertical-align:top;border-top:0;border-right:0;border-bottom:0;border-left:0"">
                                                                        <table class=""image_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0"">
                                                                            <tr>
                                                                                <td class=""pad"" style=""padding-bottom:30px;padding-top:25px;width:100%;padding-right:0;padding-left:0"">
                                                                                    <div class=""alignment"" align=""center"" style=""line-height:10px"">
                                                                                        <div style=""max-width:158px"">
                                                                                            <img src=""https://d15k2d11r6t6rl.cloudfront.net/pub/r388/l239mmxz/ia0/otr/bc8/AIS-logo.png"" style=""display:block;height:auto;border:0;width:100%"" width=""158"" alt=""Alternate text"" title=""Alternate text"" height=""auto"">
                                                                                        </div>
                                                                                    </div>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                        <table class=""text_block block-2"" width=""100%"" border=""0"" cellpadding=""10"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0;word-break:break-word"">
                                                                            <tr>
                                                                                <td class=""pad"">
                                                                                    <div style=""font-family:sans-serif"">
                                                                                        <div class style=""font-size:12px;font-family:Tahoma,Verdana,Segoe,sans-serif;mso-line-height-alt:14.399999999999999px;color:#000;line-height:1.2"">
                                                                                            <p style=""margin:0;font-size:14px;text-align:center;mso-line-height-alt:16.8px"">
                                                                                                <span style=""word-break: break-word; font-size: 26px;"">Complete your account activation!</span>
                                                                                            </p>
                                                                                        </div>
                                                                                    </div>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                        <table class=""text_block block-3"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0;word-break:break-word"">
                                                                            <tr>
                                                                                <td class=""pad"" style=""padding-bottom:25px;padding-left:10px;padding-right:10px;padding-top:20px"">
                                                                                    <div style=""font-family:sans-serif"">
                                                                                        <div class style=""font-size:12px;font-family:Tahoma,Verdana,Segoe,sans-serif;mso-line-height-alt:14.399999999999999px;color:#8c8c8c;line-height:1.2"">
                                                                                            <p style=""margin:0;font-size:14px;text-align:center;mso-line-height-alt:16.8px"">
                                                                                                <span style=""word-break: break-word; font-size: 17px;"">Click the button below to<strong> confirm your email</strong>:</span>
                                                                                            </p>
                                                                                        </div>
                                                                                    </div>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                        <table class=""button_block block-4"" width=""100%"" border=""0"" cellpadding=""10"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0"">
                                                                            <tr>
                                                                                <td class=""pad"">
                                                                                    <div class=""alignment"" align=""center"">
                                                                                        <div style=""background-color:#005a9c;border-bottom:0 solid transparent;border-left:0 solid transparent;border-radius:50px;border-right:0 solid transparent;border-top:0 solid transparent;color:#fff;display:block;font-family:Tahoma,Verdana,Segoe,sans-serif;font-size:18px;font-weight:700;mso-border-alt:none;padding-bottom:10px;padding-top:10px;text-align:center;text-decoration:none;width:35%;word-break:keep-all"">
                                                                                            <span style=""word-break: break-word; padding-left: 20px; padding-right: 20px; font-size: 18px; display: inline-block; letter-spacing: normal;""><span style=""word-break: break-word; line-height: 36px;""><strong><a href=""{tokenLink}"" style=""color:#fff;text-decoration:none;"">Confirm</a></strong></span></span>
                                                                                        </div>
                                                                                    </div>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                        <table class=""social_block block-5"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0"">
                                                                            <tr>
                                                                                <td class=""pad"" style=""padding-bottom:10px;padding-left:10px;padding-right:10px;padding-top:25px;text-align:center"">
                                                                                    <div class=""alignment"" align=""center"">
                                                                                        <table class=""social-table"" width=""108px"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0;display:inline-block"">
                                                                                            <tr>
                                                                                                <td style=""padding:0 2px 0 2px""><a href=""https://www.facebook.com/"" target=""_blank""><img src=""https://app-rsrc.getbee.io/public/resources/social-networks-icon-sets/t-only-logo-default-gray/facebook@2x.png"" width=""32"" height=""auto"" alt=""Facebook"" title=""Facebook"" style=""display:block;height:auto;border:0""></a></td>
                                                                                                <td style=""padding:0 2px 0 2px""><a href=""https://twitter.com/"" target=""_blank""><img src=""https://app-rsrc.getbee.io/public/resources/social-networks-icon-sets/t-only-logo-default-gray/twitter@2x.png"" width=""32"" height=""auto"" alt=""Twitter"" title=""Twitter"" style=""display:block;height:auto;border:0""></a></td>
                                                                                                <td style=""padding:0 2px 0 2px""><a href=""https://instagram.com/"" target=""_blank""><img src=""https://app-rsrc.getbee.io/public/resources/social-networks-icon-sets/t-only-logo-default-gray/instagram@2x.png"" width=""32"" height=""auto"" alt=""Instagram"" title=""Instagram"" style=""display:block;height:auto;border:0""></a></td>
                                                                                            </tr>
                                                                                        </table>
                                                                                    </div>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </body>";

                    #endregion

                    Response response = _mailHelper.SendEmail(model.Username, "Email Confirmation!", emailBody);

                    if (response.IsSuccess)
                    {
                        return ConfirmEmail("Confirm Email", "The instructions to activate your account have been sent to your email!");
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

        public async Task<IActionResult> EmailConfirmation(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                RedirectToAction("PageNotFound", "Home");
            }

            var user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                RedirectToAction("PageNotFound", "Home");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                RedirectToAction("PageNotFound", "Home");
            }

            return ConfirmEmail("Email Confirmed", "Your account activation is complete! You can now log in.");
        }

        public IActionResult NotAuthorized()
        {
            return View("Error", new ErrorViewModel { ErrorMessage = "User not authorized!", RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult UserNotFound()
        {
            return View("Error", new ErrorViewModel { ErrorMessage = "User not found!", RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ConfirmEmail(string title, string message)
        {
            return View("ConfirmEmail", new EmailConfirmationViewModel { Title = title, Message = message });
        }
    }
}
