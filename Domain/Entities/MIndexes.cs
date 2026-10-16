using Domain.CommonModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities
{
    public class MIndexes : Common
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Enter Index Name")]
        [StringLength(50)]
        [RegularExpression(
            @"^(?![0-9]+$)(?!\.+$)(?!_+$)[A-Za-z0-9._ ]+$",
            ErrorMessage = "Use letters, numbers, spaces, dots, or underscores. The value cannot contain only numbers, only dots, or only underscores."
        )]
        public required string Index_Name { get; set; }

        [Required]
        [Display(Name = "Enter IP Address")]
        [StringLength(50)]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\.){3}(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)$",
            ErrorMessage = "Enter a valid IPv4 address, for example 192.168.1.10."
        )]
        public required string IpAddress { get; set; }

        [Required]
        public required string Key { get; set; }

    }
}
