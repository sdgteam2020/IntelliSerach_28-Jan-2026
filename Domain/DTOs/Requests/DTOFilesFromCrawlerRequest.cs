using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Requests
{
    public class DTOFilesFromCrawlerRequest
    {
        public required string CrawlerId { get; set; }
        public required string FileName { get; set; }
    }
}
