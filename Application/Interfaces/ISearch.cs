using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface ISearch
    {
        Task<string> GetResponse(DTOSerchRequest Request, string Url, string UserName, string Password);
        Task<List<DTOIndexesDetailsResponse>> IndexesDetails(string Url, string UserName, string Password);
        Task<List<FileSource>> GetDocDetailsByIndexName(string Url, string indexName, string UserName, string Password);
        
    }
}
