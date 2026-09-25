using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response
{
    public class DTODocCountByIndexNameResponse
    {
        public int Total { get; set; }
        public double Size  { get; set; }

        public int Ocr_Files { get; set; }
        public int Non_Ocr_Files { get; set; }

        public List<DTOFileExtensionBucket> File_Extensions { get; set; }
    }
}
