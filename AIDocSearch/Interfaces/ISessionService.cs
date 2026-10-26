using Domain.CommonModel;
using Microsoft.AspNetCore.Http;
using System;

namespace AIDocSearch.Interfaces
{
    public interface ISessionService
    {
        string EnsureSalt(HttpContext httpContext);
        void SetUserSession(HttpContext httpContext, DTOSession session);
        void SetAPITokenSession(HttpContext httpContext, DTOSessionForAPI session);
        void ClearSession(HttpContext httpContext);
    }
}
