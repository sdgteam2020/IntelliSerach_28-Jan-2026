using Domain.CommonModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities
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
        [Required]
        [Display(Name = "Index Name")]
        [StringLength(50)]
        [RegularExpression(@"^(?!\d+$)(?!\.+$)(?!_+$)[A-Za-z0-9._\s]+$", ErrorMessage = "Letters, numbers, spaces, . and _ are allowed, but the value cannot be only numbers, only dots, or only underscores.")]
        public required string Index_Name { get; set; }
    }
}
