using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface ISearch
    {
        Task<string> GetResponse(DTOSerchRequest Request, string Url, string encodedKey);
        Task<List<DTOIndexesDetailsResponse>> IndexesDetails(string Url, string encodedKey);
        Task<List<FileSource>> GetDocDetailsByIndexName(string Url, string indexName, int OcrType, string encodedKey);
        Task<DTODocCountByIndexNameResponse> GetDocCountByIndexName(string Url, string indexName, string encodedKey);
        Task<List<DTOStatisticsHit>> GetstatisticsByIndexName(string Url, string indexName, string encodedKey);
       // Task<DTOGetDoc_WebCountResponse> GetCountDoc_Web(string baseUrl, string userName, string password);
        Task<DTOGetDoc_WebCountResponse> GetCountDoc_Web(string baseUrl, string encodedKey);
    }
}
