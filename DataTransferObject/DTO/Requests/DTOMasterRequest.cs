namespace DataTransferObject.DTO.Requests
{
    public class DTOMasterRequest
    {
        public string tableName { get; set; }
        public int? id { get; set; }
        public string? ParentId { get; set; }
    }
}