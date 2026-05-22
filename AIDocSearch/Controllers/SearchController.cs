using BusinessLogicsLayer.ScraperAPI;
using BusinessLogicsLayer.ScraperSetting;
using BusinessLogicsLayer.SearchContent;
using BusinessLogicsLayer.UploadPdf;
using DataTransferObject.Constants;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private readonly IAPI _aPI;
        
        public SearchController(IHttpClientFactory httpClientFactory, IConfiguration _configuration, ISearch _searchService, IWebHostEnvironment env, IUploadFiles uploadFiles, IAPI aPI)
        {
            _httpClientFactory = httpClientFactory;
            this._searchService = _searchService;
            this._configuration = _configuration;
            _env = env;
            _uploadFiles = uploadFiles;
            _aPI = aPI;
            
        }

        public async Task<IActionResult> Index(string searchInput)
        {
            ViewBag.query = searchInput;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchContent([FromBody] DTOSerchRequest Data)
        {
            if (ModelState.IsValid)
            {
                if (Data == null || string.IsNullOrEmpty(Data.DataString))
                {
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ConnKeyConstants.BadRequestMessage, "Invalid search request."));
                    // return BadRequest("Invalid search request.");
                }
                string Url = _configuration["UrlData:Url"] ?? string.Empty;
                string UserName = _configuration["UrlData:UserName"] ?? string.Empty;
                string Password = _configuration["UrlData:Password"] ?? string.Empty;
                // Call the search service to get the response
                var RetData = await _searchService.GetResponse(Data, Url, UserName, Password);
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

        public async Task<IActionResult> IndexesDetails()
        {
            string Url = _configuration["UrlData:indicesUrl"] ?? string.Empty;
            string UserName = _configuration["UrlData:UserName"] ?? string.Empty;
            string Password = _configuration["UrlData:Password"] ?? string.Empty;
            // Call the search service to get the response
           
            return View(await _searchService.IndexesDetails(Url, UserName, Password));
        }

        public async Task<IActionResult> Upload()
        {
            var allData = await _uploadFiles.GetUploadFileByUserId(
      Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier))
        );

            ViewBag.AllData = allData.Data;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(DTOUploadRequest Data)
        {
            if (ModelState.IsValid)
            {
                string FilePath = Path.Combine(_env.WebRootPath, "UploadForIndexing");
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Data.FileName == null)
                {
                    ModelState.AddModelError(string.Empty, "Please select a file to upload.");
                    return View(Data);
                }
                var ret = await _uploadFiles.UploadFileAsync(Data.FileName, FilePath, Convert.ToInt32(userId));
                if (ret.Code == 200)
                {
                    if (!IsFSCrawlerRunning())
                    {
                        RunFSCrawler();
                    }
                    TempData["SuccessMessage"] = "File uploaded successfully.";
                    return RedirectToAction(nameof(Upload)); // PRG pattern
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ret.Message ?? "File upload failed.");
                    return View(Data);
                }
            }

            return View(Data);
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
        public async Task<IActionResult> GetFilter()
        {
            if (ModelState.IsValid)
            {
               
                DTOAPILoginRequest dTOAPILoginRequest = new DTOAPILoginRequest();
                dTOAPILoginRequest.username = "admin";
                dTOAPILoginRequest.password = "Admin@123";
                string APILoginURL = _configuration["UrlData:APILoginURL"] ?? string.Empty;
                var ret = await _aPI.Getauthentication(dTOAPILoginRequest, APILoginURL);
                if (ret != null && ret.Status == true)
                {
                    DTOWebScraperDataRequest dTOScraperDataRequest = new DTOWebScraperDataRequest();
                    dTOScraperDataRequest.CSRFToken = ret.CSRFToken;
                    dTOScraperDataRequest.session_key = ret.session_key;
                    string APIuniqueurls = _configuration["UrlData:APIuniqueurls"] ?? string.Empty;
                    var ret1 = await _aPI.GetFilter(dTOScraperDataRequest, APIuniqueurls);
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.ScraperingMessage, ret1));
                }
                else
                {
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.InternalServerError, ConnKeyConstants.InternalServerErrorMessage, ""));
                }
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

        public async Task<IActionResult> IndexMap()
        {
            return View();

        }

    }
}