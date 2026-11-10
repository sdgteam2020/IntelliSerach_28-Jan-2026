using AIDocSearch.Helpers;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Domain.CommonModel;
using Domain.Constants;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Domain.Entities;
using Infrastructure.Shared.Helpers;
using Infrastructure.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;

namespace AIDocSearch.Controllers
{
    [Authorize] // Require authentication for all actions in this controller
    public class SearchController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISearch _searchService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IUploadFiles _uploadFiles;
        private readonly IUpload _upload;
        private readonly IAPI _aPI;
        private readonly IAESEncrytDecry _AESEncrytDecry;
        private readonly ICurrentUserService _currentUserService;
        public SearchController(IHttpClientFactory httpClientFactory, IConfiguration _configuration, ISearch _searchService, IWebHostEnvironment env, IUploadFiles uploadFiles, IAPI aPI, IUpload upload, IAESEncrytDecry aESEncrytDecry, ICurrentUserService currentUserService)
        {
            _httpClientFactory = httpClientFactory;
            this._searchService = _searchService;
            this._configuration = _configuration;
            _env = env;
            _uploadFiles = uploadFiles;
            _upload = upload;
            _aPI = aPI;
            _AESEncrytDecry = aESEncrytDecry;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(string searchInput)
        {
            ViewBag.query = searchInput;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchContent([FromBody] EncryptedRequest Payload)
        {
            DTOSerchRequest Data = await _AESEncrytDecry.DecryptAESWithDTO<DTOSerchRequest>(Payload.Data, SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").AESKey);
            if (Data == null)
                return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ConnKeyConstants.BadRequestMessage, null));

            else
            {

                ModelState.Clear();
                if (TryValidateModel(Data))

                {
                    if (Data == null || string.IsNullOrEmpty(Data.DataString))
                    {
                        return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ConnKeyConstants.BadRequestMessage, "Invalid search request."));
                        // return BadRequest("Invalid search request.");
                    }
                    string Url = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__Url") ?? string.Empty;
                    string encodedKey = SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").encodedKey;
                    // Call the search service to get the response
                    var RetData = await _searchService.GetResponse(Data, Url,encodedKey);
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessMessage, RetData));
                }
                else
                {
                    var error = ModelState.Where(x => x.Value?.Errors?.Count > 0)
                                                  .SelectMany(x => x.Value!.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();

                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.IncorrectData, ConnKeyConstants.IncorrectDataMessage, error));
                }

            }
           
        }
        #region Indexes Details Add And Assgin Index To Users
        // IndexesDetails moved to IndexController for client-side binding
        #endregion

        #region Upload Files
        public async Task<IActionResult> Upload()
        {
            await LoadUserFiles();
            
            return View();
        }
        private async Task LoadUserFiles()
        {
            var userId = Convert.ToInt32(
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _uploadFiles.GetUploadFileByUserId(userId);
            ViewBag.AllData = result.Data;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
        public async Task<IActionResult> Upload(DTOUploadRequest Data)
        {
            const long maxFileSize = 5 * 1024 * 1024;

            if (Data.FileName == null)
            {
                ModelState.AddModelError(
                    nameof(Data.FileName),
                    "Please select a PDF file.");
            }
            else
            {
                if (Data.FileName.Length == 0)
                {
                    ModelState.AddModelError(
                        nameof(Data.FileName),
                        "Empty files are not allowed.");
                }

                if (Data.FileName.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        nameof(Data.FileName),
                        "File size must not exceed 5 MB.");
                }

                if (!string.Equals(
                        Path.GetExtension(Data.FileName.FileName),
                        ".pdf",
                        StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        nameof(Data.FileName),
                        "Only PDF files are allowed.");
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                string filePath = Path.Combine(
                    _env.WebRootPath,
                    "UploadForIndexing");

                var session = SessionHeplers.GetObject<DTOSession>(
                    HttpContext.Session, "Users");

                if (session == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Session expired. Please login again."
                    });
                }

                string userName = session.UserName;

                var ret = await _upload.UploadFileAsync(
                    Data.FileName,
                    filePath,
                    Convert.ToInt32(_currentUserService.UserId),
                    userName);

                if (ret.Code == 200)
                {
                    return Json(new
                    {
                        success = true,
                        message = "File uploaded successfully."
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = ret.Message ?? "File upload failed."
                });
            }
            catch (Exception)
            {
                // Log the exception using your application's logger.

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading the file."
                });
            }
        }
        public bool IsFSCrawlerRunning()
        {
            return Process.GetProcesses()
                .Any(p => p.ProcessName.ToLower().Contains("fscrawler"));
        }
        public void RunFSCrawler()
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = @"C:\Elk\fscrawler-distribution-2.10-SNAPSHOT";
            process.StartInfo.Arguments = "/c fscrawler --config_dir ./new asdc_new --loop 1";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            Console.WriteLine(output);
            Console.WriteLine(error);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> DeleteFiles([FromBody] DTOCommon data)
        {
            // Validate request
            if (data.Id <= 0)
                return Json(new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    ConnKeyConstants.FileDeleteFailed,
                    null));

            // Get file record from database
            var file = await _uploadFiles.Get(data.Id);

            // Check if record exists
            if (file == null)
                return Json(new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    ConnKeyConstants.FileNotExists,
                    null));

            // Verify that the current user owns the file
            if (file.UpdatedBy != _currentUserService.UserId)
                return Json(new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    ConnKeyConstants.NotAuth,
                    null));

            // Physical upload directory path
            string uploadPath = Path.Combine(_env.WebRootPath, "UploadForIndexing");

            // Check whether the physical file exists
            bool fileExists = await _upload.CheckFileExits(uploadPath, file.FileName);

            if (!fileExists)
                return Json(new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    ConnKeyConstants.FileNotExists,
                    null));

            // Delete database record
            var deletedRecord = await _uploadFiles.DeleteFilesAndData(data.Id);

            // Verify database deletion succeeded
            if (deletedRecord == null)
                return Json(new DTOGenericResponse<object>(
                    ConnKeyConstants.BadRequest,
                    ConnKeyConstants.FileDeleteFailed,
                    null));

            // Delete physical file from server
            bool fileDeleted = await _upload.FileDelete(uploadPath, deletedRecord.FileName);

            // Return final response
            return Json(new DTOGenericResponse<object>(
                fileDeleted ? ConnKeyConstants.Success : ConnKeyConstants.BadRequest,
                fileDeleted ? ConnKeyConstants.FileDeleteSucess : ConnKeyConstants.FileDeleteFailed,
                null));
        }
        #endregion Upload Files
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetFilter()
        {
            if (ModelState.IsValid)
            {
                string Url = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__indicesUrl") ?? string.Empty;
                string encodedKey = SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").encodedKey;
                var IndexesDetails = await _searchService.IndexesDetails(Url, encodedKey);

                // Call the search service to get the response
                return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessMessage, IndexesDetails.Where(i => i.index.Contains("fs-")).ToList()));
                

            
            }
            else
            {
                var error = ModelState.Where(x => x.Value?.Errors?.Count > 0)
                                              .SelectMany(x => x.Value!.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();

                return Json(new DTOGenericResponse<object>(ConnKeyConstants.IncorrectData, ConnKeyConstants.IncorrectDataMessage, error));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetDoc_webCount()
        {
            string Url = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__BaseUrl") ?? string.Empty;
            string UserName = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__UserName") ?? string.Empty;
            string Password = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__Password") ?? string.Empty;
           string encodedKey=SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").encodedKey;
            var IndexesDetails = await _searchService.GetCountDoc_Web(Url, encodedKey);
            return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessMessage, IndexesDetails));
        }
        public async Task<IActionResult> IndexMap()
        {
            return View();

        }

    }
}