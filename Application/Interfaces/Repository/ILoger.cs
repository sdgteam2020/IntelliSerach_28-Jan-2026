using Application.Interfaces.GenericRepository;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repository
{
    public interface ILoger : IGenericRepository<ExceptionLog>
    {
        Task<DTOGenericResponse<object>> AddAsync(DTOExceptionLogRequest Log);
    }
}
