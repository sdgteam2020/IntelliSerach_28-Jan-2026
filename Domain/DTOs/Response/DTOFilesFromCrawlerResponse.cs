using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response
{
    public class DTOFilesFromCrawlerResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
