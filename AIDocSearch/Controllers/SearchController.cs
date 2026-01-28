using BusinessLogicsLayer.SearchContent;
using BusinessLogicsLayer.UploadPdf;
using DataTransferObject.Constants;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
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
        public SearchController(IHttpClientFactory httpClientFactory, IConfiguration _configuration, ISearch _searchService, IWebHostEnvironment env, IUploadFiles uploadFiles)
        {
            _httpClientFactory = httpClientFactory;
            this._searchService = _searchService;
            this._configuration = _configuration;
            _env = env;
            _uploadFiles = uploadFiles;
        }
        public async Task<IActionResult> Index(string searchInput)
        {
            ViewBag.query = searchInput;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchContent([FromBody]DTOSerchRequest Data)
        {
            if (ModelState.IsValid)
            {
                if (Data == null || string.IsNullOrEmpty(Data.DataString))
                {
                    return BadRequest("Invalid search request.");
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

                var ret= await _uploadFiles.UploadFileAsync(Data.FileName, FilePath, Convert.ToInt32(userId));
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

    }
}
