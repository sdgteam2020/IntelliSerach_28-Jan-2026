using Domain.DTOs.Requests;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public class RegisterResult
    {
        public bool Succeeded { get; set; }
        public string[] Errors { get; set; } = System.Array.Empty<string>();
    }

    public interface IRegisterService
    {
        Task<RegisterResult> RegisterAsync(DTORegisterRequest model, string plainPassword);
    }
}
