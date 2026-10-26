using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.DTOs.Response.API
{
    public class DTOCrawlerCrawlErrorResponse
    {

        public int count { get; set; }
        public List<CrawlPageDto> pages { get; set; }
        public int Code { get; set; }
        public string message { get; set; }
    }
    public class CrawlPageDto
    {
        public string url { get; set; }

        public string reason { get; set; }

        public CrawlPageDetailsDto details { get; set; }
    }

    public class CrawlPageDetailsDto
    {
        public string found_on { get; set; }
        public string status { get; set; } // Optional: will be null for items that don't have a status field
    }
}
