using Application.Interfaces;
using Domain.DTOs.Requests;
using Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AIDocSearch.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<RegisterService> _logger;

        public RegisterService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ILogger<RegisterService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<RegisterResult> RegisterAsync(DTORegisterRequest model, string plainPassword)
        {
            var result = new RegisterResult();

            var existing = await _userManager.FindByNameAsync(model.UserName);
            if (existing != null)
            {
                result.Succeeded = false;
                result.Errors = new[] { Domain.Constants.ConnKeyConstants.UserAlreadyExistsMessage };
                return result;
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.UserName + "@army.mil",
                Active = false,
                Updatedby = 1,
                UpdatedOn = System.DateTime.UtcNow,
                Name = model.Name,
                RankId = model.RankId
            };

            var createResult = await _userManager.CreateAsync(user, plainPassword);
            if (!createResult.Succeeded)
            {
                result.Succeeded = false;
                result.Errors = createResult.Errors.Select(e => e.Description).ToArray();
                return result;
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = model.Role });
            }

            await _userManager.AddToRoleAsync(user, model.Role);
            var claims = new[] { new Claim("Role", model.Role) };
            await _userManager.AddClaimsAsync(user, claims);

            // Add login info and registration token similar to previous behavior
            var loginInfo = new UserLoginInfo("IntelliSearch", user.Id.ToString(), "Indian Army IntelliSearch");
            await _userManager.AddLoginAsync(user, loginInfo);
            await _userManager.SetAuthenticationTokenAsync(user, "IntelliSearch", "RegistrationToken", System.Guid.NewGuid().ToString());

            result.Succeeded = true;
            return result;
        }
    }
}
