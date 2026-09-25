using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.DTOs.Response
{
    public class DTOIndexesDetailsResponse
    {
        public string health { get; set; }
        public string status { get; set; }
        public string index { get; set; }
        public string uuid { get; set; }

        public int pri { get; set; }
        public int rep { get; set; }

        [JsonProperty("docs.count")]
        public long DocsCount { get; set; }

        [JsonProperty("docs.deleted")]
        public long DocsDeleted { get; set; }

        [JsonProperty("store.size")]
        public string StoreSize { get; set; }

        [JsonProperty("pri.store.size")]
        public string PriStoreSize { get; set; }

        [JsonProperty("dataset.size")]
        public string DatasetSize { get; set; }
        public string message { get; set; }
    }
    public class ElasticUniqueFileResponse
    {
        public ElasticAggregations aggregations { get; set; }
    }


    public class ElasticAggregations
    {
        public IndexAggregation indices { get; set; }
    }


    public class IndexAggregation
    {
        public List<ElasticBucket> buckets { get; set; }
    }


    public class ElasticBucket
    {
        public string key { get; set; }

        public long doc_count { get; set; }

        public UrlCount url_count { get; set; }

        public UniqueFiles unique_files { get; set; }
    }


    public class UrlCount
    {
        public long doc_count { get; set; }

        public CountValue count { get; set; }
    }


    public class UniqueFiles
    {
        public long doc_count { get; set; }

        public CountValue count { get; set; }
    }


    public class CountValue
    {
        public long value { get; set; }
    }
}
