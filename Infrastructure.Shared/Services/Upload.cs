using Application.Interfaces;
using Application.Interfaces.Repository;
using Domain.Constants;
using Domain.DTOs.Response;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Exceptions;
namespace Infrastructure.Shared.Services
{
    public class Upload : IUpload
    {
        private readonly IUploadService _service;
        private readonly IUploadFiles _uploadFilesDB;

        public Upload(IUploadService service, IUploadFiles uploadFilesDB)
        {
            _service = service;
            _uploadFilesDB = uploadFilesDB;
        }

        public async Task<bool> CheckFileExits(string uploadPath, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    return false;

                var filePath = Path.Combine(uploadPath, fileName);

                if (File.Exists(filePath))
                {
                   
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> CheckFileExits(string Path)
        {
            try
            {
   

                if (File.Exists(Path))
                {

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> FileDelete(string uploadPath,string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    return false;

                var filePath = Path.Combine(uploadPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        private static async Task<string?> ValidatePdfAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "The uploaded file is empty.";

            // Check the PDF header. A .pdf extension or MIME type alone isn't proof.
            byte[] header = new byte[5];

            await using (var stream = file.OpenReadStream())
            {
                int bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length));

                if (bytesRead < 5 ||
                    !header.AsSpan().SequenceEqual("%PDF-"u8))
                {
                    return "The file is not a valid PDF.";
                }
            }

            // 2. Reject Windows Short File Names (SFN / 8.3)
            // Examples: FILE~1.PDF, DOCUMENT~2.PDF
            string fileName = Path.GetFileName(file.FileName ?? string.Empty);
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (Regex.IsMatch(Path.GetFileNameWithoutExtension(fileName), @"^[A-Za-z0-9_-]+~[0-9]+$", RegexOptions.IgnoreCase))
            {
                return "Please rename your file using only letters, numbers, spaces, underscores (_), and hyphens (-). Special characters (like #, $, or ~) are not allowed.";
            }
            try
            {
                await using var stream = file.OpenReadStream();
                using var document = PdfDocument.Open(stream);

                if (document.NumberOfPages == 0)
                    return "The PDF contains no pages.";

                // Reject PDFs with no extractable text on any page.
                // Image-only/scanned PDFs will also be rejected by this check.
                bool containsText = document.GetPages()
                    .Any(page => !string.IsNullOrWhiteSpace(page.Text));

                if (!containsText)
                    return "The PDF is blank or contains no extractable text.";

                return null; // Valid for the checks above.
            }
            catch (PdfDocumentEncryptedException)
            {
                return "Password-protected PDFs are not allowed.";
            }
            catch (Exception)
            {
                return "The PDF is damaged, invalid, or cannot be opened.";
            }
        }
        public async Task<DTOGenericResponse<object>> UploadFileAsync( IFormFile file,string uploadPath,int userId,string userName)
        {
            if (file == null)
            {
                return new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    "No file selected.",
                    false);
            }

            // Create user folder
            string userFolder = Path.Combine(uploadPath, userName);

            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            // 5 MB = 5 * 1024 * 1024 bytes
            const long MaxFileSize = 5 * 1024 * 1024;

            if (file.Length > MaxFileSize)
            {
                return new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    "File size must be less than 5 MB.",
                    false);
            }
            string? validationError = await ValidatePdfAsync(file);

            if (validationError != null)
            {
                return new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest, validationError, false);
            }

            string originalName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);

            string fileName = $"{originalName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

            bool pdfContentResult = _service.IsPdf(file);

            if (!pdfContentResult)
            {
                return new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    ConnKeyConstants.NotPdfFile,
                    false);
            }

            // Upload into user folder
            string savedFileName = _service.ProcessUploadedFile(
                file,
                userFolder,
                fileName);

            var now = DateTime.UtcNow;

            TrnUploadFiles trnUploadFiles = new TrnUploadFiles
            {
                FileName = userName + "\\" + savedFileName,
                OrignalFileName = file.FileName,
                CreatedOn = now,
                UpdatedOn = now,
                CreatedBy = userId,
                UpdatedBy = userId,
                IsActive = true,
                IsDeleted = false
            };

            await _uploadFilesDB.AddWithReturn(trnUploadFiles);

            return new DTOGenericResponse<object>(
                ConnKeyConstants.Success,
                ConnKeyConstants.UploadSucess,
                true);
        }
    }
}