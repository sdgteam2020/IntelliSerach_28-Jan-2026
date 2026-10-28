using AIDocSearch.Helpers;
using AIDocSearch.Interfaces;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Domain.CommonModel;
using Domain.Constants;
using Domain.DTOs.Requests;
using Domain.DTOs.Requests.API;
using Domain.DTOs.Response;
using Domain.DTOs.Response.API;
using Domain.Entities;
using Infrastructure.Shared.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AIDocSearch.Controllers
{
    [Authorize(Roles = "Admin")]// Require authentication for all actions in this controller
    public class ScraperController : Controller
    {
        private readonly IAPI _aPI; private readonly IConfiguration _configuration;
        private readonly IWebServer _webServer;
        private readonly IWebScraperSetting _webScraperSetting;
        private readonly ISessionService _sessionService;
        private readonly IAESEncrytDecry _AESEncrytDecry;
        public ScraperController(IAPI aPI, IConfiguration configuration, IWebServer webServer, IWebScraperSetting webScraperSetting, ISessionService sessionService, IAESEncrytDecry aESEncrytDecry)
        {
            _aPI = aPI;
            _configuration = configuration;
            _webServer = webServer;
            _webScraperSetting = webScraperSetting;
            _sessionService = sessionService;
            _AESEncrytDecry = aESEncrytDecry;
        }

        public IActionResult Index()
        {
            return View();
        }

        public static string Decrypt(string cipherText, string keyB64, string ivB64)
        {
            var key = Convert.FromBase64String(keyB64);
            var iv = Convert.FromBase64String(ivB64);
            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Scraperingpayload(string payload)
        {
            DTOScraperRequest Data = await _AESEncrytDecry.DecryptAESWithDTO<DTOScraperRequest>(payload, SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").AESKey);
            if (Data == null)
                return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ConnKeyConstants.BadRequestMessage, null));

            //var key = HttpContext.Session.GetString("AES_KEY");
            //var iv = HttpContext.Session.GetString("AES_IV");

            //if (key == null || iv == null)
            //    return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, "Key expired", ""));
            //string json = "";
            //try
            //{
            //    json = Decrypt(payload, key, iv);
            //}
            //catch (CryptographicException)
            //{
            //    return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, "Payload tampered or expired", ""));
            //}
            //var Data = JsonSerializer.Deserialize<DTOScraperRequest>(json);
            if (Data == null)
                return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, "Invalid payload data", ""));
            ModelState.Clear();
            if (TryValidateModel(Data))
            {
                return await Scrapering(Data);

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
        public async Task<IActionResult> Scrapering(DTOScraperRequest Data)
        {


            if (ModelState.IsValid)
            {
                WebScraperSetting ScraperSetting = await _webScraperSetting.GetById(1);
                DTOAPILoginRequest dTOAPILoginRequest = new DTOAPILoginRequest();
                dTOAPILoginRequest.client_id = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__APILoginclient_id") ?? string.Empty;
                dTOAPILoginRequest.client_secret = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__APILoginclient_secret") ?? string.Empty;
                string APILoginURL = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__APILoginURL") ?? string.Empty;
                var ret = await _aPI.Getauthentication(dTOAPILoginRequest, APILoginURL);
                if (ret.Status && !string.IsNullOrEmpty(ret.access_token))
                {
                    DTOWebScraperDataRequest dTOWebScraperDataRequest = new DTOWebScraperDataRequest();
                    //dTOWebScraperDataRequest.CSRFToken = ret.CSRFToken;
                    dTOWebScraperDataRequest.session_key = ret.access_token;
                    dTOWebScraperDataRequest.url = Data.Url;
                    dTOWebScraperDataRequest.max_pages = ScraperSetting.max_pages;
                    dTOWebScraperDataRequest.Abbreviation = Data.Abbreviation;
                    string APIcrawlseoURL = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__APIcrawlseoURL") ?? string.Empty;
                    DTOSessionForAPI dTOSessionForAPI = new DTOSessionForAPI();
                    dTOSessionForAPI.access_token = ret.access_token;
                    _sessionService.SetAPITokenSession(HttpContext, dTOSessionForAPI);
                    var ret1 = await _aPI.GetData(dTOWebScraperDataRequest, APIcrawlseoURL);
                    if (ret1 == null || ret1.Code == 400)
                        return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ret1.Message, ret1.Message));
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.ScraperingMessage, ret1));
                }
                
                else
                {
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.InternalServerError, ConnKeyConstants.InternalServerErrorMessage, ConnKeyConstants.InternalServerErrorMessage));
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckCrawlStatus(string Job_Id)
        {
            if (string.IsNullOrWhiteSpace(Job_Id))
                return BadRequest("Job ID is required.");

            var apiSession =
                SessionHeplers.GetObject<DTOSessionForAPI>(HttpContext.Session, "API");

            if (apiSession == null || string.IsNullOrWhiteSpace(apiSession.access_token))
                return Unauthorized("API session is missing or expired. Please authenticate again.");

            var statusUrl = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__APICrawlStatus");

            if (string.IsNullOrWhiteSpace(statusUrl))
                return StatusCode(500, "Crawl status URL is not configured.");

            var result = await _aPI.GetCrawlerCrawlStatus(
                Job_Id,
                apiSession.access_token,
                statusUrl);
           
             return Ok(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckCrawlErros(string Job_Id)
        {
            if (string.IsNullOrWhiteSpace(Job_Id))
                return BadRequest("Job ID is required.");

            var apiSession =
                SessionHeplers.GetObject<DTOSessionForAPI>(HttpContext.Session, "API");

            if (apiSession == null || string.IsNullOrWhiteSpace(apiSession.access_token))
                return Unauthorized("API session is missing or expired. Please authenticate again.");

            var statusUrl = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__APICrawlErros");

            if (string.IsNullOrWhiteSpace(statusUrl))
                return StatusCode(500, "Crawl status URL is not configured.");

            var result = await _aPI.GetCrawlerCrawlErros(
                Job_Id,
                apiSession.access_token,
                statusUrl);

            return Ok(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckCrawlPdf(string Job_Id)
        {
            if (string.IsNullOrWhiteSpace(Job_Id))
                return BadRequest("Job ID is required.");

            var apiSession =
                SessionHeplers.GetObject<DTOSessionForAPI>(HttpContext.Session, "API");

            if (apiSession == null || string.IsNullOrWhiteSpace(apiSession.access_token))
                return Unauthorized("API session is missing or expired. Please authenticate again.");

            var statusUrl = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__APICrawlPdf");

            if (string.IsNullOrWhiteSpace(statusUrl))
                return StatusCode(500, "Crawl status URL is not configured.");

            var result = await _aPI.GetCrawlerPdf(
                Job_Id,
                apiSession.access_token,
                statusUrl);

            return Ok(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckCrawlDownloadPdf(string Job_Id)
        {
            if (string.IsNullOrWhiteSpace(Job_Id))
                return BadRequest("Job ID is required.");

            var apiSession =
                SessionHeplers.GetObject<DTOSessionForAPI>(HttpContext.Session, "API");

            if (apiSession == null || string.IsNullOrWhiteSpace(apiSession.access_token))
                return Unauthorized("API session is missing or expired. Please authenticate again.");

            var statusUrl = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__APICrawlDownloadPdf");

            if (string.IsNullOrWhiteSpace(statusUrl))
                return StatusCode(500, "Crawl status URL is not configured.");

            DTOCrawlPdfDownloadRequest dTOCrawlPdfDownloadRequest = new DTOCrawlPdfDownloadRequest();
            dTOCrawlPdfDownloadRequest.job_id = Job_Id;
            dTOCrawlPdfDownloadRequest.Token = apiSession.access_token;
            dTOCrawlPdfDownloadRequest.Url = statusUrl;
            dTOCrawlPdfDownloadRequest.verify_ssl= false;
            dTOCrawlPdfDownloadRequest.max_files=1000;
            dTOCrawlPdfDownloadRequest.overwrite_existing= false;
            dTOCrawlPdfDownloadRequest.max_pdf_size_mb = 50;
            dTOCrawlPdfDownloadRequest.replace_if_size_changed = true;

            var result = await _aPI.GetCrawlerDownloadPdf(dTOCrawlPdfDownloadRequest);

            return Ok(result);
        }
        #region Add Server

        public async Task<IActionResult> AddWebServer()
        {
            var allData = await _webServer.GetAll();

            ViewBag.AllData = allData;
            TempData["SuccessMessage"] = null;
            TempData["ErrorMessage"] = null;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWebServer(DTOAddWebServerRequest Request)
        {
            var allData = await _webServer.GetAll();

            ViewBag.AllData = allData;
            bool isDuplicate = allData.Any(i =>
                 i.Id != Request.Id &&
                 !string.IsNullOrWhiteSpace(i.Includes) &&
                 !string.IsNullOrWhiteSpace(Request.Includes) &&
                 i.Includes.Trim().Equals(Request.Includes.Trim(), StringComparison.OrdinalIgnoreCase)
             );

            if (isDuplicate)
            {
                ModelState.AddModelError(nameof(Request.Includes), $"{Request.Includes} already exists.");
                TempData["ErrorMessage"] = $"{Request.Includes} already exists.";
                return View(Request);
            }
            if (ModelState.IsValid)
            {
                var UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var now = DateTime.UtcNow;
                TrnWebServer data;

                if (Request.Id > 0)
                {
                    // 🔹 UPDATE
                    data = await _webServer.GetById(Convert.ToInt32(Request.Id)); // fetch existing record

                    if (data == null)
                    {
                        ModelState.AddModelError(string.Empty, "Record not found.");
                        TempData["ErrorMessage"] = $"Record not found.";
                        return View(Request);
                    }

                    // Keep original Created fields
                    data.Url = Request.Url;
                    data.Includes = Request.Includes;
                    data.Index_Name = Request.Index_Name;
                    data.UpdatedOn = now;
                    data.UpdatedBy = UserId;
                }
                else
                {
                    // 🔹 INSERT
                    data = new TrnWebServer
                    {
                        Url = Request.Url,
                        Includes = Request.Includes,
                        Index_Name = Request.Index_Name,
                        CreatedOn = now,
                        UpdatedOn = now,
                        CreatedBy = UserId,
                        UpdatedBy = UserId,
                        IsActive = true,
                        IsDeleted = false
                    };
                }

                TrnWebServer ret = await _webServer.AddWebServer(data);
                if (ret != null && ret.Id > 0)
                {
                    TempData["SuccessMessage"] = "Record Save successfully.";
                    return View(Request);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ret.Url ?? "Record Not Save.");
                    TempData["ErrorMessage"] = $"Record Not Save.";
                    return View(data);
                }
            }



            return View(Request);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAllWebServer()
        {
            return Json(await _webServer.GetAll());
        }




        #endregion
    }

}
