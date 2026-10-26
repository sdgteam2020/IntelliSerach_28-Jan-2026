using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response.API
{
    public class DTOPdfCrawlResultResponse
    {
        public string job_id { get; set; }
        public string index_name { get; set; }
        public string alias { get; set; }
        public int total_pdf_links_found { get; set; }
        public List<PdfLinkItemDto> pdf_links { get; set; }
        public int Code { get; set; }
        public string message { get; set; }
    }
    public class PdfLinkItemDto
    {
        public string page_url { get; set; }
        public string pdf_url { get; set; }
        public string pdf_name { get; set; }
    }
}
