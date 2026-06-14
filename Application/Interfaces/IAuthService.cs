using Domain.IdentityEntities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsNotAllowed { get; set; }
        public string[] Errors { get; set; } = System.Array.Empty<string>();
        public ApplicationUser? User { get; set; }
    }

    public interface IAuthService
    {
        Task<AuthResult> SignInAsync(string userName, string password, string role, CancellationToken cancellationToken = default);
        Task SignOutAsync();
    }
}
