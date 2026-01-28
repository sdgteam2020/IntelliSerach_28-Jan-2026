using DataTransferObject.DTO.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicsLayer.SearchContent
{
    public interface ISearch
    {
        Task<string> GetResponse(DTOSerchRequest Request,string Url, string UserName, string Password);
    }
}
