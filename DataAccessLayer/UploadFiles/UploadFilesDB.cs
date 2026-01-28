using DataAccessLayer.GenericRepository;
using DataTransferObject.Constants;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.UploadFiles
{
    public class UploadFilesDB : GenericRepository<TrnUploadFiles>, IUploadFilesDB
    {
        protected new readonly ApplicationDbContext _context;
        public UploadFilesDB(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<DTOGenericResponse<object>> GetUploadFileByUserId(int UserId)
        {
            var Data = await _context.trnUploadFiles.Where(i => i.CreatedBy == UserId).ToListAsync();
            return new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessGet, Data);
        }
    }
}
