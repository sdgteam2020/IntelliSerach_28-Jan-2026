using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs.Response
{
    public class ElasticsearchSearchResponse
    {
        public Hits Hits { get; set; }
    }

    public class Hits
    {
        public List<Hit> HitsList { get; set; }

        [JsonProperty("hits")]
        public List<Hit> Hitss { get; set; }
    }

    public class Hit
    {
        [JsonProperty("_source")]
        public FileSource Source { get; set; }
    }

    public class FileSource
    {
        public FileInfoData File { get; set; }
        public PathData Path { get; set; }
        public string url { get; set; }
    }

    public class FileInfoData
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
    }

    public class PathData
    {
        public string Real { get; set; }
    }
}
