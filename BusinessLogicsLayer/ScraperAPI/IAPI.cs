using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicsLayer.ScraperAPI
{
    public interface IAPI
    {
        public Task<DTOLoginAPIResponse> Getauthentication(DTOAPILoginRequest Data);
        public Task<DTOScraperDataResponse> GetData(DTOScraperDataRequest Data);
    }
}
