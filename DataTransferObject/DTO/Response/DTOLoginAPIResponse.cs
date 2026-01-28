using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject.DTO.Response
{
    public class DTOLoginAPIResponse
    {
        public string message { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string session_key { get; set; } = string.Empty;
        public string CSRFToken { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
        public string Message { get; set; } = string.Empty;
      
    }
}
