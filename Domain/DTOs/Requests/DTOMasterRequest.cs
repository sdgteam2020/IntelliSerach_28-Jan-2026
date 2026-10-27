using Domain.Localize;
using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Requests
{
    public class DTOMasterRequest
    {
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(250, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        public string tableName { get; set; }
        public int? id { get; set; }
        public string? ParentId { get; set; }
    }
}