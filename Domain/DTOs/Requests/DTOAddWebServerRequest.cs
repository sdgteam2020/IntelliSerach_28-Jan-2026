using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.DTOs.Requests
{
    public class DTOAddWebServerRequest
    {
        public int? Id { get; set; }
        [Required]
        [RegularExpression(@"^(https?:\/\/)?([\w-]+\.)+[\w-]+(\/[\w\-._~:/?#[\]@!$&'()*+,;=%]*)?$", ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Enter Url")]
        public required string Url { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_/-:/]{1,50}$", ErrorMessage = "Only alphabets, numbers, _, /,: and - allowed. Maximum 50 characters.")]
        [Display(Name = "alias")]
        public required string Includes { get; set; }
        [Required]
        [Display(Name = "Index Name")]
        [StringLength(50)]
        [RegularExpression(@"^(?!\d+$)(?!\.+$)(?!_+$)[A-Za-z0-9._\s]+$", ErrorMessage = "Letters, numbers, spaces, . and _ are allowed, but the value cannot be only numbers, only dots, or only underscores.")]
        public required string Index_Name { get; set; }
    }
}
