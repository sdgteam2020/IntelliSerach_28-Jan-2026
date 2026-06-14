using System.Collections.Generic;

namespace Application.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? UserName { get; }
        IEnumerable<string> Roles { get; }
    }
}
