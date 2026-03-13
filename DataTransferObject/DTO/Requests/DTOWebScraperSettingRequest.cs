using System;
using System.Collections.Generic;
using System.Text;

namespace DataTransferObject.DTO.Requests
{
    public class DTOWebScraperSettingRequest
    {
        public int? Id { get; set; }
        public int max_pdfs { get; set; }
        public int max_pages { get; set; }
    }
}
