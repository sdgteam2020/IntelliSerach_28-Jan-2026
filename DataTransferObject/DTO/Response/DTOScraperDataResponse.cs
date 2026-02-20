namespace DataTransferObject.DTO.Response
{
    public class DTOScraperDataResponse
    {
        public string status { get; set; }
        public ScraperData data { get; set; }

        public string message { get; set; } = string.Empty;
    }

    public class ScraperData
    {
        public string website_url { get; set; }
        public int downloaded_count { get; set; }
        public List<ScraperDocument> documents { get; set; }
        public string download_directory { get; set; }
    }

    public class ScraperDocument
    {
        public string url { get; set; }
        public string path { get; set; }
        public string filename { get; set; }
        public DateTime downloaded_at { get; set; }
        public string source_page { get; set; }
        public string method { get; set; }
        public decimal size_kb { get; set; }
        public string downloaded_display { get; set; }
    }
}