using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject.DTO.Requests
{
    public class DTOScraperRequest
    {
        [Required]
        [RegularExpression(
     @"^(https?:\/\/)?([\w-]+\.)+[\w-]+(\/[\w\-._~:/?#[\]@!$&'()*+,;=%]*)?$",
     ErrorMessage = "Please enter a valid URL"
 )]
        [Display(Name = "Enter Url")]
        public string Url { get; set; }

    }
}
