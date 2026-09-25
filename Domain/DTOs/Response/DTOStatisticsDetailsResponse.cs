using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response
{
    public class DTOStatisticsDetailsResponse
    {
        public DTOStatisticsDetailsHits Hits { get; set; } = new();
    }


    public class DTOIndexingStatisticsResponse
    {
        public DTOStatisticsDetailsHits Hits { get; set; } = new();
    }


    public class DTOStatisticsDetailsHits
    {
        [JsonProperty("hits")]
        public List<DTOStatisticsHit> Hits { get; set; } = new();
    }


    public class DTOStatisticsHit
    {
        [JsonProperty("_source")]
        public DTOIndexingStatistics Source { get; set; } = new();
    }


    public class DTOIndexingStatistics
    {
        [JsonProperty("indexname")]
        public string IndexName { get; set; } = string.Empty;


        [JsonProperty("totalfile")]
        public long TotalFile { get; set; }


        [JsonProperty("allowedExtensionSet")]
        public HashSet<string> AllowedExtensionSet { get; set; } = new();


        [JsonProperty("scantime")]
        public DateTime ScanTime { get; set; }
    }
}
