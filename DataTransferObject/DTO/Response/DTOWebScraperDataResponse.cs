using System.Text.Json.Serialization;

namespace DataTransferObject.DTO.Response
{
    public class DTOWebScraperDataResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("run_id")]
        public int RunId { get; set; }

        [JsonPropertyName("pages_crawled")]
        public int PagesCrawled { get; set; }

        [JsonPropertyName("data")]
        public List<ScraperWebDataDto>? Data { get; set; }

        [JsonPropertyName("logs")]
        public List<string>? Logs { get; set; }

        public string message { get; set; } = string.Empty;
    }

    public class ScraperWebDataDto
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("canonical_url")]
        public string? CanonicalUrl { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        //[JsonPropertyName("content")]
        //public string? Content { get; set; }

        //[JsonPropertyName("content_length")]
        //public int ContentLength { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        //[JsonPropertyName("h1")]
        //public string? H1 { get; set; }

        //[JsonPropertyName("headings_h1")]
        //public List<string>? HeadingsH1 { get; set; }

        //[JsonPropertyName("headings_h2")]
        //public List<string>? HeadingsH2 { get; set; }

        //[JsonPropertyName("headings_h3")]
        //public List<string>? HeadingsH3 { get; set; }

        //[JsonPropertyName("meta_description")]
        //public string? MetaDescription { get; set; }

        //[JsonPropertyName("meta_keywords")]
        //public string? MetaKeywords { get; set; }

        //[JsonPropertyName("lang")]
        //public string? Lang { get; set; }

        //[JsonPropertyName("crawled_at")]
        //public DateTime CrawledAt { get; set; }

        //[JsonPropertyName("clicks_total")]
        //public int ClicksTotal { get; set; }

        //[JsonPropertyName("recent_clicks")]
        //public double RecentClicks { get; set; }

        //[JsonPropertyName("ranking_score")]
        //public double RankingScore { get; set; }

        //[JsonPropertyName("last_clicked_at")]
        //public DateTime? LastClickedAt { get; set; }

        //[JsonPropertyName("last_clicked_at_ms")]
        //public long? LastClickedAtMs { get; set; }
    }
}