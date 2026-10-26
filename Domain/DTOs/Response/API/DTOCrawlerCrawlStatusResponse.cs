using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.DTOs.Response.API
{
    public class DTOCrawlerCrawlStatusResponse
    {
     
        public string? job_id { get; set; }

       
        public string? status { get; set; }


        public string? start_url { get; set; }


        public string? normalized_url { get; set; }


        public string? alias { get; set; }

        public string? index_name { get; set; }

        public int pages_crawled { get; set; }

        public int docs_indexed { get; set; }

        public int links_found { get; set; }

        public int errors { get; set; }

        public string? message { get; set; }
        public int Code { get; set; }
    }
}
