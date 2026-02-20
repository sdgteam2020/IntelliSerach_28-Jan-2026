using BusinessLogicsLayer.Ranks;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;

namespace BusinessLogicsLayer.UnitOfWorks
{
    public interface IUnitOfWork
    {
        IRank Rank { get; }

        public Task<List<DTOMasterResponse>> GetAllMMaster(DTOMasterRequest Data);
    }
}