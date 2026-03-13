using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DataTransferObject.DTO.Response
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
}
