using Infrastructure.Shared.Helpers;
using Domain.CommonModel;
using Domain.Constants;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Domain.IdentityEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Application.Interfaces.Repository;
using Application.Interfaces;
using AIDocSearch.Interfaces;

namespace AIDocSearch.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        private readonly IUserAccount _account;
        public const string SessionKeySalt = "_Salt";
        public readonly IRank _rank;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuthService _authService;
        private readonly IRegisterService _registerService;
        private readonly ISessionService _sessionService;
        private readonly IAESEncrytDecry _AESEncrytDecry;
        private readonly IUserRepository _iUserRepository;

        public AccountController(ILogger<AccountController> logger, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IUserAccount _account, IRank rank, IEncryptionService encryptionService, IAuthService authService, IRegisterService registerService, ISessionService sessionService, IAESEncrytDecry aESEncrytDecry, IUserRepository iUserRepository)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            _logger = logger;
            this.signInManager = signInManager;
            this._account = _account;
            _rank = rank;
            _encryptionService = encryptionService;
            _authService = authService;
            _registerService = registerService;
            _sessionService = sessionService;
            _AESEncrytDecry = aESEncrytDecry;
            _iUserRepository = iUserRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            // Ensure a session salt exists and sign out any existing user
            var salt = _sessionService.EnsureSalt(HttpContext);
            ViewBag.hdns = salt;
            await _authService.SignOutAsync();

            var model = new DTOLoginRequest { UserName = "Admin" };
            return View(model);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(DTOLoginRequest model, string? returnUrl, CancellationToken cancellationToken)
        {
            
            try
            {
                string? GetSalt = HttpContext.Session.GetString(SessionKeySalt); // Get Salt from Session
                if (!string.IsNullOrEmpty(GetSalt))
                {
                    ViewBag.hdns = GetSalt;
                    model.Password = _encryptionService.Decrypt(model.Password, GetSalt);
                    model.UserName = _encryptionService.Decrypt(model.UserName, GetSalt);
                }
                if (!ModelState.IsValid)
                    return View(model);

                // Get the user's IP address and the current request URL (for logging or auditing)
                string ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var url = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";

                // Attempt to find the user by username
                // Use AuthService only for sign-in operation below; we still need user for role checks and account state
                var selectedUser = await userManager.FindByNameAsync(model.UserName);
                if (selectedUser == null)
                {
                    // User not found -> redirect to registration flow
                    TempData["UserName"] = model.UserName;
                    TempData["RoleName"] = model.RoleName;
                    return RedirectToAction("Register", "Account");
                }

                if (!selectedUser.Active)
                {
                    return RedirectToAction("ContactUs", "Account");
                }
                if (string.IsNullOrWhiteSpace(selectedUser.UserName))
                    throw new InvalidOperationException("UserName cannot be null.");
                // Get the user's roles
                var roles = await userManager.GetRolesAsync(selectedUser);

                if (roles == null || !roles.Any(r => r.Equals(model.RoleName, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(string.Empty, ConnKeyConstants.RoleMismatchMessage);
                    return View(model);
                }

                // Attempt to sign in the user with the provided password
                var authResult = await _authService.SignInAsync(model.UserName, model.Password, model.RoleName, cancellationToken);
                if (authResult.Succeeded)
                {
                    _sessionService.ClearSession(HttpContext);

                    // Delete the session cookie so ASP.NET Core issues a NEW session ID
                    var sessionCookieName = ".AspNetCore.Session"; // or ".MOU.Session" if you renamed it
                    if (Request.Cookies.ContainsKey(sessionCookieName))
                    {
                        Response.Cookies.Delete(sessionCookieName);
                    }

                    await userManager.UpdateSecurityStampAsync(selectedUser);

                    // Re-issue sign-in with fresh stamp
                    await signInManager.SignInAsync(selectedUser, isPersistent: false);
                    // If a valid returnUrl is provided, redirect to it
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    var ret = await _rank.GetByshort(selectedUser.RankId);
                    if (ret == null)
                    {
                        // Fallback to a safe default
                        ret = new Domain.Entities.MRank { RankAbbreviation = "N/A" };
                    }


                    var dTOSession = new DTOSession
                    {
                        UserId = selectedUser.Id,
                        RoleName = string.Join(",", await userManager.GetRolesAsync(selectedUser)),
                        UserName = selectedUser.UserName,
                        Name = selectedUser.Name,
                        RankName = ret.RankAbbreviation,
                        AESKey = _AESEncrytDecry.GenerateKey()

                    };
                    _sessionService.SetUserSession(HttpContext, dTOSession);
                    ViewBag.Message = ConnKeyConstants.LoggedInMessage;
                    // Redirect users with the "User" role to the Dashboard
                    return RedirectToActionPermanent("Dashboard", "Home");
                }
                else if (authResult.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, ConnKeyConstants.AccountLockedMessage);
                }
                else if (authResult.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, string.Format(ConnKeyConstants.SignInNotAllowedFormat, selectedUser.UserName));
                }
                else
                {
                    // Do not expose internal counters to the user; log detail instead
                    _logger.LogWarning("Failed login for user {User} from {IP}. AccessFailedCount={Count}", selectedUser.UserName, ipAddress, selectedUser.AccessFailedCount);
                    ModelState.AddModelError(string.Empty, ConnKeyConstants.InvalidCredentialsMessage);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Login attempt canceled by client for user {User}", model.UserName);
                ModelState.AddModelError(string.Empty, ConnKeyConstants.RequestCanceledMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {User}", model.UserName);
                ModelState.AddModelError(string.Empty, ConnKeyConstants.InternalServerErrorMessage);
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
            string GetSalt = _AESEncrytDecry.GenerateKey();
            HttpContext.Session.SetString(SessionKeySalt, GetSalt);
            ViewBag.hdns = GetSalt;
            ViewBag.UserName = TempData["UserName"] as string;
            ViewBag.RoleName = TempData["RoleName"] as string;
            //if (string.IsNullOrEmpty(TempData["UserName"] as string))
            //    return RedirectToAction("Login");
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(DTORegisterRequest model)
        {
            ViewBag.UserName = model.UserName;
           
            // Use session service to ensure/get salt
            string? GetSalt = _sessionService.EnsureSalt(HttpContext); // ensures and returns salt
            if (GetSalt != null)
            {
                ViewBag.hdns = GetSalt;
            }
            else
            {
                ModelState.AddModelError(string.Empty, ConnKeyConstants.SaltNullMessage);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Decrypt the incoming password using the injected encryption service
                string Password = _encryptionService.Decrypt(model.ConfirmPassword, GetSalt);
                model.ConfirmPassword = Password;

                // Delegate registration logic to the injected register service
                var registerResult = await _registerService.RegisterAsync(model, Password);
                if (registerResult.Succeeded)
                {
                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("ContactUs", "Account");
                }

                foreach (var error in registerResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            return View(model);
        }
        [Authorize(Roles = "Admin")]

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetSalt()
        {
            var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)); // 256-bit
            var iv = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

            HttpContext.Session.SetString("AES_KEY", key);
            HttpContext.Session.SetString("AES_IV", iv);

            return Ok(new { key, iv });
        }
    }
}