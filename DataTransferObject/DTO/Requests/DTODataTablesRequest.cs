using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject.DTO.Requests
{
    public class DTODataTablesRequest
    {
        [RegularExpression("^[0-9]+$", ErrorMessage = "Numbers allowed.")]
        public int Draw { get; set; }

        [RegularExpression("^[0-9]+$", ErrorMessage = "Numbers allowed.")]
        public int Start { get; set; }

        [RegularExpression("^[0-9]+$", ErrorMessage = "Numbers allowed.")]
        public int Length { get; set; }

        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Only Alphabets and Numbers allowed.")]
        public string? searchValue { get; set; }

        [RegularExpression(@"^[a-zA-Z_]*$", ErrorMessage = "Only alphabets and underscores are allowed.")]
        public string sortColumn { get; set; } = string.Empty;

        [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "Only Alphabets allowed.")]
        public string sortDirection { get; set; } = string.Empty;

        [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "Only Alphabets allowed.")]
        public string Choice { get; set; } = string.Empty;

     
    }
}
