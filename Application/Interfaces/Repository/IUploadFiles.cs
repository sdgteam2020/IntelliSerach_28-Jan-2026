using Application.Interfaces.GenericRepository;
using Domain.DTOs.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repository
{
    public interface IUploadFiles : IGenericRepository<TrnUploadFiles>
    {
        Task<DTOGenericResponse<object>> GetUploadFileByUserId(int UserId);
        Task<TrnUploadFiles> DeleteFilesAndData(int UploadId);
    }
}
