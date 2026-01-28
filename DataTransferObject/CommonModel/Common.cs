using DataTransferObject.IdentityModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject.CommonModel
{
    public class Common
    {


        [Required]
        [DataType(DataType.Date)]
        [Column(TypeName = "datetime")]
        public DateTime? CreatedOn { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        [Required]
        [DataType(DataType.Date)]
        [Column(TypeName = "datetime")]
        public DateTime? UpdatedOn { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));


        public int? CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public ApplicationUser? CreatedByUser { get; set; }


        public int? UpdatedBy { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public ApplicationUser? UpdatedByUser { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
