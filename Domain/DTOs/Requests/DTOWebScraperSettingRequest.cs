using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.DTOs.Requests
{
    public class DTOWebScraperSettingRequest
    {
        public int? Id { get; set; }
        [Range(0, 3000, ErrorMessage = "The maximum value for max_pdfs cannot exceed 3000.")]
        public int max_pdfs { get; set; }

        [Range(0, 3000, ErrorMessage = "The maximum value for max_pages cannot exceed 3000.")]
        public int max_pages { get; set; }
    }
}
