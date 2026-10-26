using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Requests
{
    public class DTORegisterCrawler
    {
        public string IndexerKey { get; set; } = string.Empty;
        public string SystemIp { get; set; } = string.Empty;
        public string CrawlerId { get; set; }
    }
}
