using BusinessLogicsLayer.Loger;
using DataAccessLayer;
using DataAccessLayer.GenericRepository;
using DataAccessLayer.Logers;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;

namespace BusinessLogicsLayer.Logers
{
    public class Loger : GenericRepository<ExceptionLog>, ILoger
    {
        private readonly ILogerDB _ILoger;

        public Loger(ApplicationDbContext context, ILogerDB iLoger) : base(context)
        {
            _ILoger = iLoger;
        }

        public Task<DTOGenericResponse<object>> AddAsync(DTOExceptionLogRequest Log)
        {
            return _ILoger.AddAsync(Log);
        }
    }
}