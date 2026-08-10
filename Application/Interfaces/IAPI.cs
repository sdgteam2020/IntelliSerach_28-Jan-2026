using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IAPI
    {
        public Task<DTOLoginAPIResponse> Getauthentication(DTOAPILoginRequest Data, string APILoginURL);

        public Task<DTOScraperDataResponse> GetData(DTOScraperDataRequest Data, string APIcrawlURL);

        public Task<DTOScrapyCrawlResponse> GetData(DTOWebScraperDataRequest Data, string APIcrawlseoURL);

        Task<DTOFilterResponse> GetFilter(DTOWebScraperDataRequest Data, string APIuniqueurls);
    }
}
