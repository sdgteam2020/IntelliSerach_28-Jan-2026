using Domain.Constants;
using Domain.DTOs.Response;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Repository.GenericRepository;
using Application.Interfaces.Repository;

namespace Infrastructure.Repository
{
    public class UploadFilesRepository : GenericRepository<TrnUploadFiles>, IUploadFiles
    {
        protected new readonly ApplicationDbContext _context;

        public UploadFilesRepository(ApplicationDbContext context) : base(context)
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