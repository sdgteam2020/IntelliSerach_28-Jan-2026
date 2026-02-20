namespace DataTransferObject.CommonModel
{
    public class DTOSession
    {
        public int UserId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string RankName { get; set; } = string.Empty;
    }
}