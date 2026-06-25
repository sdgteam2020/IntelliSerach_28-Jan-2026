using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.DTOs.Response
{
    public class DTOWebServerResponse
    {
        public int Id { get; set; }

        public required string Url { get; set; }

       
        public required string Includes { get; set; }
        public required string Index_Name { get; set; }
    }
}
