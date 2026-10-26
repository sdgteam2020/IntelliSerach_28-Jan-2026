using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Requests.API
{
    public class DTOCrawlPdfDownloadRequest
    {
        public string Token { get; set; }
        public string Url { get; set; }
        public string job_id { get; set; }
        public bool verify_ssl { get; set; }
        public int max_files { get; set; }
        public bool overwrite_existing { get; set; }
        public int max_pdf_size_mb { get; set; }
        public bool replace_if_size_changed { get; set; }

    }
}
