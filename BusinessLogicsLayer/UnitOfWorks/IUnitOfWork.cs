using BusinessLogicsLayer.Ranks;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicsLayer.UnitOfWorks
{
    public interface IUnitOfWork
    {
        IRank Rank { get; }
        public Task<List<DTOMasterResponse>> GetAllMMaster(DTOMasterRequest Data);
    }
}
