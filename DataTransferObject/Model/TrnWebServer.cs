using DataTransferObject.CommonModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTransferObject.Model
{
    public class TrnWebServer : Common
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^(https?:\/\/)?([\w-]+\.)+[\w-]+(\/[\w\-._~:/?#[\]@!$&'()*+,;=%]*)?$", ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Enter Url")]
        public required string Url { get; set; }

        [Required]
        [StringLength(50)]
        public required string Includes { get; set; }
    }
}
