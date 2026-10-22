using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IElasticAuthApi
    {
        Task<DTOElasticApiKeyResponse> LoginElastic(DTOElasticApiKeyRequest dTOElasticApiKeyRequest, string baseUrl, string username, string password);
    }
}
