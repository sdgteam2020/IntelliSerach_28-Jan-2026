using System.Text.Json.Serialization;

namespace DataTransferObject.DTO.Response
{
    using System.Text.Json.Serialization;

    public class DTOFilterResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<FilterItem> Items { get; set; } = new();

        [JsonPropertyName("message")]
        public string? message { get; set; }
    }

    public class FilterItem
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("abbr")]
        public string Abbr { get; set; } = string.Empty;
    }
}