using DataTransferObject.Localize;
using System.ComponentModel.DataAnnotations;

namespace DataTransferObject.DTO.Requests
{
    public class DTOLoginRequest
    {
        [Required(ErrorMessage = "Domain Name is required.")]
        [StringLength(250)]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(250, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(250)]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(250, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        public string Password { get; set; } = string.Empty;

        [RegularExpression(@"^[\w ]+$", ErrorMessage = "Invalid Role format")]
        [Required(ErrorMessage = "Role Is required")]
        public string RoleName { get; set; } = string.Empty;
    }
}