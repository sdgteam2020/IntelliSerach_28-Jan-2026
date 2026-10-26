using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response.API
{
    public class DTOCrawlerPdfDownloadSummary
    {
        public string job_id { get; set; }
        public string index_name { get; set; }
        public string alias { get; set; }
        public int total_pdf_links_found { get; set; }
        public int downloaded { get; set; }
        public int replaced_existing { get; set; }
        public int skipped_existing { get; set; }
        public int password_protected { get; set; }
        public int oversized { get; set; }
        public int corrupted { get; set; }
        public int invalid_pdf { get; set; }
        public int failed { get; set; }
        public int max_pdf_size_mb { get; set; }
        public int Code { get; set; }
        public string message { get; set; }
    }
}
