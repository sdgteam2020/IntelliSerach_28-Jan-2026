using DataTransferObject.CommonModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataTransferObject.Model
{
    public class TrnUploadFiles: Common
    {
        [Key]
        public int TrnUploadId { get; set; }
        [Required]
        [Column(TypeName = "varchar(500)")]
        public required string FileName { get; set; }
    }
}
