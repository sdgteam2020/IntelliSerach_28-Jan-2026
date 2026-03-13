using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;

namespace BusinessLogicsLayer.SearchContent
{
    public interface ISearch
    {
        Task<string> GetResponse(DTOSerchRequest Request, string Url, string UserName, string Password);
        Task<List<DTOIndexesDetailsResponse>> IndexesDetails(string Url, string UserName, string Password);
    }
}