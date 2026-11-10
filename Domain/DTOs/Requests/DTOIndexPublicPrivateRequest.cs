using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Requests
{
    public class DTOIndexPublicPrivateRequest
    {
        public string CrawlerId { get; set; }
        public string uuid { get; set; }
        public bool IsPrivate { get; set; }
    }
}
