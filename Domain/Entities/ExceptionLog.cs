using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ExceptionLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime OccurredAtUtc { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

     
        public string? StackTrace { get; set; }
     
    }
}