using DataTransferObject.DTO.Requests;

namespace BusinessLogicsLayer.SearchContent
{
    public interface ISearch
    {
        Task<string> GetResponse(DTOSerchRequest Request, string Url, string UserName, string Password);
    }
}