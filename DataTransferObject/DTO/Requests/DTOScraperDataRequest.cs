namespace DataTransferObject.DTO.Requests
{
    public class DTOScraperDataRequest
    {
        public string? url { get; set; }
        public int max_pdfs { get; set; }

        public string Abbreviation { get; set; }
        public string session_key { get; set; }
        public string CSRFToken { get; set; }
    }
}