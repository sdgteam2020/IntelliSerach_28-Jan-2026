using AIDocSearch.Helpers;
using AIDocSearch.Interfaces;
using Application.Interfaces;
using Domain.CommonModel;
using Infrastructure.Shared.Helpers;
using Microsoft.AspNetCore.Http;
using System;

namespace AIDocSearch.Services
{
    public class SessionService : ISessionService
    {
        private readonly IAESEncrytDecry _encryptionKey;
        public SessionService(IAESEncrytDecry aESEncrytDecry)
        {
            _encryptionKey= aESEncrytDecry;
        }
        public string EnsureSalt(HttpContext httpContext)
        {
            const string SessionKeySalt = "_Salt";
            var salt = httpContext.Session.GetString(SessionKeySalt);
            if (string.IsNullOrEmpty(salt))
            {
                salt = _encryptionKey.GenerateKey();
                httpContext.Session.SetString(SessionKeySalt, salt);
            }
            return salt;
        }

        public void SetUserSession(HttpContext httpContext, DTOSession session)
        {
            SessionHeplers.SetObject(httpContext.Session, "Users", session);
        }

        public void ClearSession(HttpContext httpContext)
        {
            httpContext.Session.Clear();
        }
    }
}
