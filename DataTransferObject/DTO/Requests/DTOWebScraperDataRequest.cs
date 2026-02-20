namespace DataTransferObject.DTO.Requests
{
    public class DTOWebScraperDataRequest
    {
        public string? url { get; set; }
        public int max_pages { get; set; }
        public string Abbreviation { get; set; }
        public string session_key { get; set; }
        public string CSRFToken { get; set; }
    }
}