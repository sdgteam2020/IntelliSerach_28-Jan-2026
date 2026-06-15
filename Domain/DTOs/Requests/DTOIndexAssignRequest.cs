using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Requests
{
    public class DTOIndexAssignRequest
    {
        [Required]
        public required string IndexId { get; set; }

        [Required]
        public required List<int> UserIds { get; set; }
    }
}
