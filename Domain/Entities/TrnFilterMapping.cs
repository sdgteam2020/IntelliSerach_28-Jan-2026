using Domain.CommonModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities
{
    public class TrnFilterMapping: Common
    {
        [Key]
        public int Id { get; set; }
        public required string IndexNames { get; set; }
        public string Url { get; set; }
    }
}
