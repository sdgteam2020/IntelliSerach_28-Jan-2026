using DataAccessLayer.GenericRepository;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Logers
{
    public interface ILogerDB : IGenericRepository<ExceptionLog>
    {
        Task<DTOGenericResponse<object>> AddAsync(DTOExceptionLogRequest Log);
    }
}
