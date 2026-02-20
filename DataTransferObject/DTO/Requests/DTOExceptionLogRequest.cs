namespace DataTransferObject.DTO.Requests
{
    public class DTOExceptionLogRequest
    {
        public int Id { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? Source { get; set; }

        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Endpoint { get; set; }

        public string HttpMethod { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? QueryString { get; set; }

        public string? SessionUser { get; set; }   // from DTOSession.UserName
        public string? Roles { get; set; }         // from DTOSession.RoleName

        public string? RemoteIp { get; set; }
        public string? UserAgent { get; set; }
    }
}