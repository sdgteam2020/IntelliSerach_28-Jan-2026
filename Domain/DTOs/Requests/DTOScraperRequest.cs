using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Requests
{
    public class DTOScraperRequest
    {
        [Required]
        [RegularExpression(@"^(https?:\/\/)?(([\w-]+\.)+[\w-]+|(\d{1,3}\.){3}\d{1,3})(\/[\w\-._~:/?#[\]@!$&'()*+,;=%]*)?$",
ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Enter Url")]
        public required string Url { get; set; }

        [Required]
        [RegularExpression(
    @"^[a-zA-Z0-9_]{5,20}$",
    ErrorMessage = "Only letters, numbers, and underscores are allowed. Enter 5 to 20 characters."
)]
        public required string Abbreviation { get; set; }

        public bool IsPdf { get; set; }

        public int max_pages { get; set; }
    }
}