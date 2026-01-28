using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransferObject.Localize;
using DataTransferObject.Model;

namespace DataTransferObject.IdentityModel
{
    public class ApplicationUser : IdentityUser<int>
    {
        [StringLength(50)]
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredError")]
        [MinLength(1, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MinLengthError")]
        [MaxLength(50, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "MaxLengthError")]
        [RegularExpression(@"^[a-zA-Z]+( [a-zA-Z]+)*$", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "SpecialChars")]

        public string Name { get; set; } = string.Empty;
        [Required]
     
        [RegularExpression(@"^[\d]+$", ErrorMessage = "RankId is number.")]
        public short RankId { get; set; }
        [ForeignKey(nameof(RankId))]
        public MRank? MRank { get; set; }   // ✅ Navigation Property must be PUBLIC
        public bool Active { get; set; } = false;

        [Display(Name = "Updated By")]
        public int Updatedby { get; set; }

        [Display(Name = "Updated On")]
        [DataType(DataType.Date)]
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
    }
}
