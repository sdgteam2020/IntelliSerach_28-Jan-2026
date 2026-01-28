using DataTransferObject.Localize;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject.DTO.Requests
{
    public class DTOSerchRequest
    {
        [StringLength(250)]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(250, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [RegularExpression(@"^[\w\s\.,:/]+$", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "SpecialChars")]
        public required string DataString { get; set; }
        public int size { get; set; }
        public int from { get; set; }
    }
}
