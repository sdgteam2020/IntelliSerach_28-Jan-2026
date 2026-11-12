using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response
{
    public class DTOFileTransferResultResponse
    {
        public bool Success { get; set; }
        public string ErrorCode { get; set; } = "";
        public string Message { get; set; } = "";
        public string TransferId { get; set; } = "";
    }
}
