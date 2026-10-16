using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.DTOs.Requests
{
    public class DTOIndexesRequest
    {
        public int Id { get; init; } = 0;
        [Required]
        [StringLength(50)]
        [RegularExpression(
       @"^(?![0-9]+$)(?!\.+$)(?!_+$)[A-Za-z0-9._ ]+$",
       ErrorMessage = "Use letters, numbers, spaces, dots, or underscores. The value cannot contain only numbers, only dots, or only underscores."
   )]
        public required string Index_Name { get; init; }

        [Required]
        [StringLength(50)]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\.){3}(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)$",
            ErrorMessage = "Enter a valid IPv4 address, for example 192.168.1.10."
        )]
        public required string IpAddress { get; init; }

        
    }
}
