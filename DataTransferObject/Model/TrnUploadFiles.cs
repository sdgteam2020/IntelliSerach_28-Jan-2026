using DataTransferObject.CommonModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataTransferObject.Model
{
    public class TrnUploadFiles : Common
    {
        [Key]
        public int TrnUploadId { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string FileName { get; set; }
    }
}