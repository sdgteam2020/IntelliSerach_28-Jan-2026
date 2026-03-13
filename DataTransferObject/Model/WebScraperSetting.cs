using DataTransferObject.CommonModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;

namespace DataTransferObject.Model
{
    public class WebScraperSetting : Common
    {
        [Key]
        public int Id { get; set; }
        public int max_pdfs { get; set; }
        public int max_pages { get; set; }
    }
}
