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
            exceptionLog.Message = Log.Message?.Trim() ?? string.Empty;
            exceptionLog.StackTrace = Log.StackTrace;
            exceptionLog.Source = Log.Source;
            exceptionLog.Controller = Log.Controller;
            exceptionLog.Action = Log.Action;
            exceptionLog.Endpoint = Log.Endpoint;
            exceptionLog.HttpMethod = Log.HttpMethod;
            exceptionLog.Path = Log.Path;
            exceptionLog.QueryString = Log.QueryString;
            exceptionLog.SessionUser = Log.SessionUser;
            exceptionLog.Roles = Log.Roles;
            exceptionLog.RemoteIp = Log.RemoteIp;
            exceptionLog.UserAgent = Log.UserAgent;

            var rettrnmov = await AddWithReturn(exceptionLog);
            // Return success if both operations succeed
            return new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessMessage, rettrnmov.CorrelationId);
        }
    }
}