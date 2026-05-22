using DataTransferObject.CommonModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTransferObject.Model
{
    public class TrnFilterMapping: Common
    {
        [Key]
        public int Id { get; set; }
        public required string IndexNames { get; set; }
        public string Url { get; set; }
    }
}
