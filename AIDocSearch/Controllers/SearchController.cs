using BusinessLogicsLayer.ScraperAPI;
using BusinessLogicsLayer.SearchContent;
using BusinessLogicsLayer.UploadPdf;
using DataTransferObject.Constants;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
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
    }
}