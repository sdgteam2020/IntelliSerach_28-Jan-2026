using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject.DTO.Response
{
    public class DTOUsersResponce
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ArmyNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string RankName { get; set; } = string.Empty;
        public bool Active { get; set; } = false;
        public DateTime UpdatedOn { get; set; }
    }
}
