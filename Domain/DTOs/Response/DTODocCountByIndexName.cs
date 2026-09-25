using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response
{
    public class DTODocCountByIndexName
    {
        public DTOHits Hits { get; set; }
        public DTOAggregations Aggregations { get; set; }
    }

    public class DTOHits
    {
        public DTOHitTotal Total { get; set; }
    }

    public class DTOHitTotal
    {
        public int Value { get; set; }
        public string Relation { get; set; }
    }

    public class DTOAggregations
    {
        public DTOTotalOriginalSize Total_Original_Size { get; set; }

        public DTOOCRFiles Ocr_Files { get; set; }

        public DTONonOCRFiles Non_Ocr_Files { get; set; }

        public DTOFileExtensions File_Extensions { get; set; }
    }
    // File Extension DTO
    public class DTOFileExtensions
    {
        public List<DTOFileExtensionBucket> Buckets { get; set; }
    }
    public class DTOFileExtensionBucket
    {
        public string Key { get; set; }

        public int Doc_Count { get; set; }
    }
    public class DTOTotalOriginalSize
    {
        public double Value { get; set; }
    }

    public class DTOOCRFiles
    {
        public int Doc_Count { get; set; }
    }

    public class DTONonOCRFiles
    {
        public int Doc_Count { get; set; }
    }

}
