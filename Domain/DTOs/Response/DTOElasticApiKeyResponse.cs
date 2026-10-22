using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.DTOs.Response
{
    public class DTOElasticApiKeyResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // Unix timestamp in milliseconds; null when the key does not expire.
        [JsonPropertyName("expiration")]
        public long? Expiration { get; set; }

        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; } = string.Empty;

        [JsonPropertyName("encoded")]
        public string Encoded { get; set; } = string.Empty;
    }
}
