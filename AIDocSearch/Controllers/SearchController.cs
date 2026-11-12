using AIDocSearch.Helpers;
using AIDocSearch.Hubs;
using AIDocSearch.Services;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Domain.CommonModel;
using Domain.Constants;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Domain.Entities;
using Infrastructure.Repository;
using Infrastructure.Shared.Helpers;
using Infrastructure.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private readonly ConnectionManager _cm;
        private readonly IHubContext<CrawlerHub> _hub;
        private readonly IindexUserMapping _iindexUserMapping;
        public SearchController(IHttpClientFactory httpClientFactory, IConfiguration _configuration, ISearch _searchService, IWebHostEnvironment env, IUploadFiles uploadFiles, IAPI aPI, IUpload upload, IAESEncrytDecry aESEncrytDecry, ICurrentUserService currentUserService, ConnectionManager cm, IHubContext<CrawlerHub> hub , IindexUserMapping iindexUserMapping)
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
            _cm = cm;
            _hub = hub;
            _iindexUserMapping = iindexUserMapping;
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

        public async Task<IActionResult> Upload(
    DTOUploadRequest Data,
    CancellationToken cancellationToken)
        {
            const long maxFileSize = 5L * 1024 * 1024;

            // 1. Validate file
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

            // 2. Validate index name
            if (string.IsNullOrWhiteSpace(Data.IndexName))
            {
                ModelState.AddModelError(
                    nameof(Data.IndexName),
                    "Please select an index.");
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
                // 3. Validate session
                var session = SessionHeplers.GetObject<DTOSession>(
                    HttpContext.Session,
                    "Users");

                if (session == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Session expired. Please login again."
                    });
                }

                string userName = session.UserName;

                int userId = Convert.ToInt32(
                    _currentUserService.UserId);

                string filePath = Path.Combine(
                    _env.WebRootPath,
                    "UploadForIndexing");

                // 4. Validate PDF content
                string pdfValidationResult =
                    await _upload.ValidatePdfAsync(Data.FileName);

                if (!string.IsNullOrEmpty(pdfValidationResult))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = pdfValidationResult
                    });
                }

                DTOFileTransferResultResponse? transferResult = null;

                // 5. Transfer when index is not IntelliSearch
                if (!string.Equals(
                    Data.IndexName,
                    "IntelliSearch",
                    StringComparison.OrdinalIgnoreCase))
                {
                    string crawlerId = Data.IndexName.StartsWith(
                                "fs-", StringComparison.OrdinalIgnoreCase)
                                ? Data.IndexName
                                : "fs-" + Data.IndexName;

                    
                         int CurrentUserId = (int)_currentUserService.UserId;
                    string uuid = Data.uuid;
                    bool retExists = await _iindexUserMapping.CheckUserIndexingExists(CurrentUserId, uuid, (string)_currentUserService.RoleName);
                    if (!retExists)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "User is not assigned to the specified index."
                        });
                    }
                    transferResult = await SendFileToClient(
                       crawlerId,
                        Data.FileName,
                        cancellationToken);

                    // Return actual transfer error
                    if (!transferResult.Success)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            transferId = transferResult.TransferId,
                            errorCode = transferResult.ErrorCode,
                            message = transferResult.Message
                        });
                    }
                }

                // 6. Save uploaded PDF on server and database
                DTOGenericResponse<object> ret =
                    await _upload.UploadFileAsync(
                        Data.FileName,
                        filePath,
                        userId,
                        userName);

                // 7. Final response
                if (ret.Code == 200)
                {
                    return Json(new
                    {
                        success = true,
                        transferId = transferResult?.TransferId,
                        errorCode = "SUCCESS",
                        message = transferResult != null
                            ? "PDF uploaded and transferred to indexer successfully."
                            : "PDF uploaded successfully."
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    errorCode = "SERVER_UPLOAD_FAILED",
                    message = ret.Message ?? "File upload failed."
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    success = false,
                    errorCode = "SERVER_ERROR",
                    message = "An error occurred while uploading the file."
                });
            }
        }
        public async Task<DTOFileTransferResultResponse> SendFileToClient(
     string crawlerId,
     IFormFile file,
     CancellationToken cancellationToken = default)
        {
            string transferId = Guid.NewGuid().ToString("N");

            DTOFileTransferResultResponse Failed(
                string errorCode,
                string message)
            {
                return new DTOFileTransferResultResponse
                {
                    TransferId = transferId,
                    Success = false,
                    ErrorCode = errorCode,
                    Message = message
                };
            }

            try
            {
                // 1. Validate uploaded file
                if (file == null || file.Length == 0)
                {
                    return Failed(
                        "INVALID_FILE",
                        "Please select a valid PDF file.");
                }

                const long maxFileSize = 5L * 1024 * 1024;

                if (file.Length > maxFileSize)
                {
                    return Failed(
                        "FILE_TOO_LARGE",
                        "PDF file size must not exceed 5 MB.");
                }

                if (!string.Equals(
                        Path.GetExtension(file.FileName),
                        ".pdf",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Failed(
                        "INVALID_FILE_TYPE",
                        "Only PDF files are allowed.");
                }

                // 2. Validate indexer connection
                if (string.IsNullOrWhiteSpace(crawlerId) ||
                    !_cm.TryGetConnectionId(crawlerId, out var connectionId))
                {
                    return Failed(
                        "INDEXER_DISCONNECTED",
                        "Indexer is not connected.");
                }

                // 3. Generate safe destination filename
                string originalFileName = Path.GetFileName(
                    file.FileName.Replace('\\', '/'));

                string fileName = originalFileName;

                const int chunkSize = 64 * 1024;

                using var stream = file.OpenReadStream();

                long fileSize = file.Length;

                int totalChunks = checked(
                    (int)((fileSize + chunkSize - 1) / chunkSize));

                // 4. Start SignalR transfer
                await _hub.Clients.Client(connectionId).SendAsync(
                    "ReceiveServerFileStart",
                    transferId,
                    fileName,
                    fileSize,
                    cancellationToken);

                // 5. Send file chunks
                byte[] buffer = new byte[chunkSize];

                int chunkIndex = 0;
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(
                    buffer,
                    0,
                    buffer.Length,
                    cancellationToken)) > 0)
                {
                    string base64 = Convert.ToBase64String(
                        buffer, 0, bytesRead);

                    await _hub.Clients.Client(connectionId).SendAsync(
                        "ReceiveServerFileChunk",
                        transferId,
                        chunkIndex,
                        base64,
                        cancellationToken);

                    chunkIndex++;
                }

                // 6. Wait for client confirmation
                using var timeoutCts =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken);

                timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

                var fileUploadStatus =
                    await _hub.Clients.Client(connectionId)
                        .InvokeAsync<DTOFileTransferResultResponse>(
                            "ReceiveServerFileComplete",
                            transferId,
                            totalChunks,
                            timeoutCts.Token);

                // 7. Check null response
                if (fileUploadStatus == null)
                {
                    return Failed(
                        "NULL_RESPONSE",
                        "Client did not return a transfer result.");
                }

                return fileUploadStatus;
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return Failed(
                    "CONFIRMATION_TIMEOUT",
                    "Transfer timed out. The final client status is unknown.");
            }
            catch (OperationCanceledException)
            {
                return Failed(
                    "TRANSFER_CANCELLED",
                    "Transfer was cancelled. The final client status is unknown.");
            }
            catch (Exception ex)
            {
               

                return Failed(
                    "TRANSFER_FAILED",
                    "Unable to complete file transfer to indexer.");
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