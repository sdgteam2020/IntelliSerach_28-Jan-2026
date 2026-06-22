namespace Domain.DTOs.Requests
{
    public class DTOExceptionLogRequest
    {
        public int Id { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        
    }
}