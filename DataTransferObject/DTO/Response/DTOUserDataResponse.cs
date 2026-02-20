namespace DataTransferObject.DTO.Response
{
    public class DTOUserDataResponse
    {
        public int Id { get; set; }
        public string DomainId { get; set; } = string.Empty;

        public string? Name { get; set; }
        public bool Active { get; set; }
        public List<string>? RoleNames { get; set; }
    }
}