using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject.DTO.Requests
{
    public class DTOUserApprovalRequest
    {
        public int Id { get; set; }
        public bool Active { get; set; } = false;

    }
}
