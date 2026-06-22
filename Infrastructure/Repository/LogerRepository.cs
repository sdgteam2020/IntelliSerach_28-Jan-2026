using Application.Interfaces.Repository;
using Domain.Constants;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Domain.Entities;
using Infrastructure.Repository.GenericRepository;

namespace Infrastructure.Repository
{
    public class LogerRepository : GenericRepository<ExceptionLog>, ILoger
    {
        public LogerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<DTOGenericResponse<object>> AddAsync(DTOExceptionLogRequest Log)
        {
            ExceptionLog exceptionLog = new ExceptionLog();

            exceptionLog.OccurredAtUtc = Log.OccurredAtUtc;
            exceptionLog.CorrelationId = Log.CorrelationId;
            
            exceptionLog.StackTrace = Log.StackTrace;
           

            var rettrnmov = await AddWithReturn(exceptionLog);
            // Return success if both operations succeed
            return new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessMessage, rettrnmov.CorrelationId);
        }
    }
}