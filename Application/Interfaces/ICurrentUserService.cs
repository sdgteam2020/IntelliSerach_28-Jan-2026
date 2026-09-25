using System.Collections.Generic;

namespace Application.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? UserName { get; }
        string? RoleName { get; }
        IEnumerable<string> Roles { get; }
    }
}
