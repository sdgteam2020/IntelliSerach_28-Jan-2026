using Domain.Localize;
using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Requests
{
    public class DTOSerchRequest
    {
        [Required]
        [StringLength(250, MinimumLength = 1)]
        public string DataString { get; set; } = string.Empty;

        // Maximum records returned per request
        [Range(1, 100)]
        public int size { get; set; } = 10;

        // Pagination offset
        [Range(0, int.MaxValue)]
        public int from { get; set; } = 0;

        public string? Filter { get; set; } = "All";

        // 1 = Keyword
        // 2 = Fuzzy
        // 3 = Exact phrase
        [Range(1, 3)]
        public int Type { get; set; } = 1;
    }
}