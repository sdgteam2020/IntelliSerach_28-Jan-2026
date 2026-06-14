using Application.Interfaces;
using Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIDocSearch.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<AuthResult> SignInAsync(string userName, string password, string role, CancellationToken cancellationToken = default)
        {
            var result = new AuthResult();

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Succeeded = false;
                result.Errors = new[] { Domain.Constants.ConnKeyConstants.UserNotFoundMessage };
                return result;
            }

            var signIn = await _signInManager.PasswordSignInAsync(userName, password, isPersistent: false, lockoutOnFailure: true);

            result.Succeeded = signIn.Succeeded;
            result.IsLockedOut = signIn.IsLockedOut;
            result.IsNotAllowed = signIn.IsNotAllowed;
            result.User = user;

            if (!signIn.Succeeded)
            {
                // Provide minimal error set; details are logged
                result.Errors = new[] { Domain.Constants.ConnKeyConstants.InvalidCredentialsMessage };
                _logger.LogWarning("SignIn failed for {User}. LockedOut={Locked} NotAllowed={NotAllowed}", userName, signIn.IsLockedOut, signIn.IsNotAllowed);
            }

            return result;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
