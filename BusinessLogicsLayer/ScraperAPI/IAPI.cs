using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;

namespace BusinessLogicsLayer.ScraperAPI
{
    public interface IAPI
    {
        public Task<DTOLoginAPIResponse> Getauthentication(DTOAPILoginRequest Data, string APILoginURL);

        public Task<DTOScraperDataResponse> GetData(DTOScraperDataRequest Data, string APIcrawlURL);

        public Task<DTOWebScraperDataResponse> GetData(DTOWebScraperDataRequest Data, string APIcrawlseoURL);

        Task<DTOFilterResponse> GetFilter(DTOWebScraperDataRequest Data, string APIuniqueurls);
    }
}