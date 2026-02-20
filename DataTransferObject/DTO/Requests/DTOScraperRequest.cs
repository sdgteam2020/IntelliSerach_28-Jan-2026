using System.ComponentModel.DataAnnotations;

namespace DataTransferObject.DTO.Requests
{
    public class DTOScraperRequest
    {
        [Required]
        [RegularExpression(@"^(https?:\/\/)?([\w-]+\.)+[\w-]+(\/[\w\-._~:/?#[\]@!$&'()*+,;=%]*)?$", ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Enter Url")]
        public required string Url { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]{1,5}$", ErrorMessage = "Only alphabets and numbers allowed. Maximum 5 characters.")]
        public required string Abbreviation { get; set; }

        public bool IsPdf { get; set; }
    }
}