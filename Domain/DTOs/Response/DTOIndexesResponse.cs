using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response
{
    public class DTOIndexesResponse
    {
        public int Id { get; init; }
        public string Index_Name { get; init; } = string.Empty;
        public string IpAddress { get; init; } = string.Empty;

        // Do not return the secret key in API responses.
        public bool HasKey { get; init; }

        public DateTime? CreatedOn { get; init; }
        public DateTime? UpdatedOn { get; init; }
        public int? CreatedBy { get; init; }
        public int? UpdatedBy { get; init; }
        public bool IsActive { get; init; }
    }
}
