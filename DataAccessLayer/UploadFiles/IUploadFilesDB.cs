using DataAccessLayer.GenericRepository;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.UploadFiles
{
    public interface IUploadFilesDB :IGenericRepository<TrnUploadFiles>
    {
        Task<DTOGenericResponse<object>> GetUploadFileByUserId(int UserId);
    }
}
