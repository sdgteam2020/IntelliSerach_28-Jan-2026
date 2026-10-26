using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.DTOs.Response.API
{

    public sealed class DTOAuthApiResponse
    {
        [JsonPropertyName("access_token")]
        public string? access_token { get; set; }

        [JsonPropertyName("token_type")]
        public string? token_type { get; set; }

        [JsonPropertyName("expires_in")]
        public int expires_in { get; set; }

        [JsonPropertyName("session_id")]
        public string? session_id { get; set; }

        [JsonIgnore]
        public bool Status { get; set; }

        [JsonIgnore]
        public string Message { get; set; } = string.Empty;

    }
}
