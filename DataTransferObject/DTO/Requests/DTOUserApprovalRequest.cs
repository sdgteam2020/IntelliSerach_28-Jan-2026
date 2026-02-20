namespace DataTransferObject.DTO.Requests
{
    public class DTOUserApprovalRequest
    {
        public int Id { get; set; }
        public bool Active { get; set; } = false;
    }
}