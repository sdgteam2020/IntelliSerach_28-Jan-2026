using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.DTOs.Requests
{
    public class DTOUserIndexMapping
    {
        [Required]
        public required int[] UserId { get; set; }
        [Required]
        public required string IndexId { get; set; }
    }
}
