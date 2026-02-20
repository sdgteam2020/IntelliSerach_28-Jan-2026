using DataTransferObject.Localize;
using DataTransferObject.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataTransferObject.DTO.Requests
{
    public class DTORegisterRequest
    {
        [RegularExpression(@"^[a-zA-Z0-9-._@+]{5,30}$", ErrorMessage = "Invalid Domain Name format")]
        [Required(ErrorMessage = "Domain Name is required.")]
        [MinLength(5, ErrorMessage = "Minimum length of Domain Name is 5 characters.")]
        [MaxLength(30, ErrorMessage = "Maximum length of Domain Name is 30 characters.")]
        [Display(Name = "Domain Name")]
        public string UserName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(250)]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(250, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(250)]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(250, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [RegularExpression(@"^[\w ]+$", ErrorMessage = "Invalid Role format")]
        [Required(ErrorMessage = "Role Is required")]
        public string Role { get; set; } = string.Empty;

        [Required]
        [ForeignKey("MRank")]
        [RegularExpression(@"^[\d]+$", ErrorMessage = "RankId is number.")]
        public short RankId { get; set; }

        public MRank? MRank { get; set; }

        [StringLength(50)]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(50, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [RegularExpression(@"^[a-zA-Z]+( [a-zA-Z]+)*$", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Alphawithspace")]
        public string Name { get; set; } = string.Empty;
    }
}