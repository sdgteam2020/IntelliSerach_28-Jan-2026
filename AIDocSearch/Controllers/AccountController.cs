using AIDocSearch.Helpers;
using AIDocSearch.Models;
using Azure.Core;
using BusinessLogicsLayer.Account;
using BusinessLogicsLayer.Ranks;
using DataTransferObject.CommonModel;
using DataTransferObject.Constants;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace AIDocSearch.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IAccount _account; 
        public const string SessionKeySalt = "_Salt";
        public readonly IRank _rank;
        public AccountController(ILogger<AccountController> logger, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,IAccount _account, IRank rank)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            _logger = logger;
            this.signInManager = signInManager;
            this._account = _account;
            _rank=rank;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            string GetSalt = AESEncrytDecry.GenerateKey();
            HttpContext.Session.SetString(SessionKeySalt, GetSalt);
            ViewBag.hdns = GetSalt;
            await signInManager.SignOutAsync();
            DTOLoginRequest model = new DTOLoginRequest();
            model.UserName = "Admin";
            return View(model); // Ensure a return statement is present
        }
        /// <summary>
        /// Handles the POST request for user login.
        /// Validates the login model, checks user credentials, and signs in the user if successful.
        /// Redirects to the specified return URL or the Dashboard for users with the "User" role.
        /// Adds appropriate error messages to the ModelState on failure.
        /// </summary>
        /// <param name="model">The login request model containing username and password.</param>
        /// <param name="returnUrl">The URL to redirect to after successful login, if provided.</param>
        /// <returns>
        /// Redirects to the returnUrl, Dashboard, or returns the login view with error messages.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(DTOLoginRequest model, string? returnUrl)
        {
            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                string? GetSalt = HttpContext.Session.GetString(SessionKeySalt); // Get Salt from Session
                if (GetSalt != null)
                {
                    ViewBag.hdns = GetSalt;
                    string Password = AESEncrytDecry.DecryptAES(model.Password, GetSalt);  //decrypt password
                    model.Password = Password;
                    string username = AESEncrytDecry.DecryptAES(model.UserName, GetSalt);  //decrypt password
                    model.UserName = username;
                }
                // Get the user's IP address and the current request URL (for logging or auditing)
                string ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var url = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";

                // Attempt to find the user by username
                var selectedUser = await userManager.FindByNameAsync(model.UserName);
                if (selectedUser !=null && !selectedUser.Active)
                {
                    return RedirectToAction("ContactUs", "Account");
                }
                if (selectedUser == null)
                {
                    // User not found, add error to ModelState
                    // User not found, redirect to the Registration page
                    TempData["UserName"] = model.UserName;
                    TempData["RoleName"] = model.RoleName;
                    return RedirectToAction("Register", "Account");
                }

                // Get the user's roles
                var roles = await userManager.GetRolesAsync(selectedUser);

                if (roles == null || !roles.Any(r => r.ToLower() == model.RoleName.ToLower()))
                {
                    ModelState.AddModelError(string.Empty,
                        "You do not have permission to log in. Your role does not match.");
                    return View(model);
                }


                // Attempt to sign in the user with the provided password
                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, false, true);
                if (result.Succeeded)
                {
                   
                    HttpContext.Session.Clear();

                    // Delete the session cookie so ASP.NET Core issues a NEW session ID
                    var sessionCookieName = ".AspNetCore.Session"; // or ".MOU.Session" if you renamed it
                    if (Request.Cookies.ContainsKey(sessionCookieName))
                    {
                        Response.Cookies.Delete(sessionCookieName);
                    }


                    await userManager.UpdateSecurityStampAsync(selectedUser);

                    // 2) Re-issue THIS session’s cookie so it contains the fresh stamp
                    await signInManager.SignOutAsync();
                    await signInManager.SignInAsync(selectedUser, isPersistent: false);
                    // If a valid returnUrl is provided, redirect to it
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    var ret = await _rank.GetByshort(selectedUser.RankId);
                    var dTOSession = new DTOSession
                    {
                        UserId = selectedUser.Id,
                        RoleName = string.Join(",", await userManager.GetRolesAsync(selectedUser)),
                        UserName = selectedUser.UserName,
                        Name = selectedUser.Name,
                        RankName = ret.RankAbbreviation,
                    };

                    SessionHeplers.SetObject(HttpContext.Session, "Users", dTOSession);
                    ViewBag.Message = "Successfully Logged In.";
                    // Redirect users with the "User" role to the Dashboard
                    return RedirectToActionPermanent("Dashboard", "Home");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Account Locked Out Please Try after 10 minutes.");


                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Already Login \" + user.UserName + \" Please Try Some Time.");


                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Not Valid User / Password. Access Failed Count " + selectedUser.AccessFailedCount + " Max Access Attempts 3");


                }
               
            }
            // Return the login view with the model and any error messages
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
        [AllowAnonymous]

        public IActionResult Register()
        {
            string GetSalt = AESEncrytDecry.GenerateKey();
            HttpContext.Session.SetString(SessionKeySalt, GetSalt);
            ViewBag.hdns = GetSalt;
            ViewBag.UserName = TempData["UserName"] as string;
            ViewBag.RoleName = TempData["RoleName"] as string;
            if (string.IsNullOrEmpty(TempData["UserName"] as string))
                return RedirectToAction("Login");
            return View(new DTORegisterRequest());
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult RegistrationRequest()
        {
            return View(new DTORegisterRequest());
        }

        /// <summary>
        /// Handles the POST request for user registration.
        /// Validates the registration model, creates the user, assigns the selected role, and signs in the user.
        /// </summary>
        /// <param name="model">The registration request model containing user details and role.</param>
        /// <returns>
        /// Redirects to Dashboard on success, or returns the registration view with error messages.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(DTORegisterRequest model)
        {
            ViewBag.UserName = model.UserName;
            ViewBag.RoleName = model.Role;
            string? GetSalt = HttpContext.Session.GetString(SessionKeySalt); // Get Salt from Session
            if (GetSalt != null)
            {
                ViewBag.hdns = GetSalt;

            }
            if (ModelState.IsValid)
            {
                // Check if user already exists
                var existingUser = await userManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "User already exists.");
                    return View(model);
                }
               
                string Password = AESEncrytDecry.DecryptAES(model.ConfirmPassword, GetSalt);  //decrypt password
                model.ConfirmPassword = Password;
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.UserName + "@army.mil",
                    Active = false,
                    Updatedby = 1,
                    UpdatedOn = DateTime.UtcNow,
                    Name=model.Name,
                    RankId=model.RankId,

                    // model.Email
                    // Add other properties as needed
                };


                var result = await userManager.CreateAsync(user, model.ConfirmPassword);
                // Replace this block in the Register POST action after successful registration
                if (result.Succeeded)
                {
                    // Ensure the role exists
                    if (!await roleManager.RoleExistsAsync(model.Role))
                    {
                        await roleManager.CreateAsync(new ApplicationRole { Name = model.Role });
                    }

                    // Assign role to user
                    await userManager.AddToRoleAsync(user, model.Role);

                    // ✅ INSERT CLAIMS INTO AspNetUserClaims Table
                    var claims = new List<Claim>()
                    {
                        new Claim("Role", model.Role),
                       
                    };
                    await userManager.AddClaimsAsync(user, claims);
                    // ✅ Insert into AspNetUserLogins
                    var loginInfo = new UserLoginInfo(
                        loginProvider: "IntelliSearch",   // LoginProvider column
                        providerKey: user.Id.ToString(), // ProviderKey column (ensure string type)
                        providerDisplayName: "Indian Army IntelliSearch" // Provide a display name as required by the constructor
                        );

                    await userManager.AddLoginAsync(user, loginInfo);
                    // ✅ Add Token into AspNetUserTokens
                    await userManager.SetAuthenticationTokenAsync(
                        user,
                        loginProvider: "IntelliSearch",
                        tokenName: "RegistrationToken",
                        tokenValue: Guid.NewGuid().ToString()
                    );


                    // Redirect to the same page (Register) after successful registration
                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("ContactUs", "Account");

                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> GetAllUser(DTODataTablesRequest request)
        {
            
            return Json(await _account.GetAllUsers(request));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult ContactUs()
        {
            return View(); // Create an AccessDenied.cshtml under Views/Account
        }
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken] // add this if not using AJAX; for AJAX use header token
        public async Task<IActionResult> UpdateApprovalStatus([FromBody] DTOUserApprovalRequest dTOUserApprovalRequest)
        {
            if (ModelState.IsValid)
            {
                // Get the user ID from the ClaimsPrincipal (the logged-in user)
              
                var data = await _account.Get(dTOUserApprovalRequest.Id);
                if (data != null)
                {
                    data.Active = dTOUserApprovalRequest.Active;
                    var retdata = await _account.UpdateWithReturn(data);
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessMessage, true));
                }

            }
            return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ConnKeyConstants.IncorrectDataMessage, true));

        }

    }
}
