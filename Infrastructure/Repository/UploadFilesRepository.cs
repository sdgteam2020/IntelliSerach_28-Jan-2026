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

        public async Task<TrnUploadFiles> DeleteFilesAndData(int UploadId)
        {
            var webServer = await _context.trnUploadFiles
                              .FirstOrDefaultAsync(x => x.TrnUploadId == UploadId);

            if (webServer == null)
                return webServer;

            webServer.IsActive = false;
            webServer.IsDeleted = true;


            await _context.SaveChangesAsync();

            return webServer;
        }

        public async Task<DTOGenericResponse<object>> GetUploadFileByUserId(int UserId)
        {
            var Data = await _context.trnUploadFiles.Where(i => i.UpdatedBy == UserId && i.IsActive==true && i.IsDeleted==false).ToListAsync();
            return new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessGet, Data);
        }

    }
}