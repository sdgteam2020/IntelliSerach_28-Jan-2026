using Domain.DTOs.Requests;
using Domain.DTOs.Requests.API;
using Domain.DTOs.Response;
using Domain.DTOs.Response.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IAPI
    {
        public Task<DTOAuthApiResponse> Getauthentication(DTOAPILoginRequest Data, string APILoginURL);

        public Task<DTOScraperDataResponse> GetData(DTOScraperDataRequest Data, string APIcrawlURL);

        public Task<DTOScrapyCrawlResponse> GetData(DTOWebScraperDataRequest Data, string APIcrawlseoURL);

        Task<DTOFilterResponse> GetFilter(DTOWebScraperDataRequest Data, string APIuniqueurls);
        Task<DTOCrawlerCrawlStatusResponse> GetCrawlerCrawlStatus(string Job_Id,string Token,string StatusUrl);
        Task<DTOCrawlerCrawlErrorResponse> GetCrawlerCrawlErros(string Job_Id,string Token,string StatusUrl);
        Task<DTOPdfCrawlResultResponse> GetCrawlerPdf(string Job_Id,string Token,string StatusUrl);
        Task<DTOCrawlerPdfDownloadSummary> GetCrawlerDownloadPdf(DTOCrawlPdfDownloadRequest Data);
    }
}
