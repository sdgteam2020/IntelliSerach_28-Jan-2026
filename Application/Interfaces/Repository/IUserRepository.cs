using Domain.IdentityEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<ApplicationUser> FindByNameAsync(String UserName);

    }
}
