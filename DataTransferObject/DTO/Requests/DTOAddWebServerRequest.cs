using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTransferObject.DTO.Requests
{
    public class DTOAddWebServerRequest
    {
        public int? Id { get; set; }
        [Required]
        [RegularExpression(@"^(https?:\/\/)?([\w-]+\.)+[\w-]+(\/[\w\-._~:/?#[\]@!$&'()*+,;=%]*)?$", ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Enter Url")]
        public required string Url { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_/-]{1,50}$", ErrorMessage = "Only alphabets, numbers, _, / and - allowed. Maximum 50 characters.")]
        [Display(Name = "alias")]
        public required string Includes { get; set; }
    }
}
