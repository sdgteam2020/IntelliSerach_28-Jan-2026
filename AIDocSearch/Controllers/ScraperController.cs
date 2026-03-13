using BusinessLogicsLayer.AddWebServer;
using BusinessLogicsLayer.ScraperAPI;
using BusinessLogicsLayer.ScraperSetting;
using DataAccessLayer.ScraperSetting;
using DataTransferObject.Constants;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIDocSearch.Controllers
{
    [Authorize] // Require authentication for all actions in this controller
    public class ScraperController : Controller
    {
        private readonly IAPI _aPI; private readonly IConfiguration _configuration;
        private readonly IWebServer _webServer;
        private readonly IWebScraperSetting _webScraperSetting;
        public ScraperController(IAPI aPI, IConfiguration configuration, IWebServer webServer, IWebScraperSetting webScraperSetting)
        {
            _aPI = aPI;
            _configuration = configuration;
            _webServer = webServer;
            _webScraperSetting = webScraperSetting;
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
            var key = HttpContext.Session.GetString("AES_KEY");
            var iv = HttpContext.Session.GetString("AES_IV");

            if (key == null || iv == null)
                return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, "Key expired", ""));
            string json = "";
            try
            {
                json = Decrypt(payload, key, iv);
            }
            catch (CryptographicException)
            {
                return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, "Payload tampered or expired", ""));
            }
            var Data = JsonSerializer.Deserialize<DTOScraperRequest>(json);
            if (Data == null)
                return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, "Invalid payload data", ""));
            return await Scrapering(Data);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Scrapering(DTOScraperRequest Data)
        {


            if (ModelState.IsValid)
            {
                WebScraperSetting ScraperSetting = await _webScraperSetting.GetById(1);
                DTOAPILoginRequest dTOAPILoginRequest = new DTOAPILoginRequest();
                dTOAPILoginRequest.username = "admin";
                dTOAPILoginRequest.password = "Admin@123";
                string APILoginURL = _configuration["UrlData:APILoginURL"] ?? string.Empty;
                var ret = await _aPI.Getauthentication(dTOAPILoginRequest, APILoginURL);
                if (ret != null && Data.IsPdf && ret.Status == true)
                {
                    DTOScraperDataRequest dTOScraperDataRequest = new DTOScraperDataRequest();
                    dTOScraperDataRequest.CSRFToken = ret.CSRFToken;
                    dTOScraperDataRequest.session_key = ret.session_key;
                    dTOScraperDataRequest.url = Data.Url;
                    dTOScraperDataRequest.max_pdfs = ScraperSetting.max_pdfs;
                    dTOScraperDataRequest.Abbreviation = Data.Abbreviation;

                    string APIcrawlURL = _configuration["UrlData:APIcrawlURL"] ?? string.Empty;
                    var ret1 = await _aPI.GetData(dTOScraperDataRequest, APIcrawlURL);
                    if (ret1.data == null)
                        return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ret1.message, ret1.message));
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.ScraperingMessage, ret1));
                }
                else if (ret != null && !Data.IsPdf && ret.Status == true)
                {
                    DTOWebScraperDataRequest dTOWebScraperDataRequest = new DTOWebScraperDataRequest();
                    dTOWebScraperDataRequest.CSRFToken = ret.CSRFToken;
                    dTOWebScraperDataRequest.session_key = ret.session_key;
                    dTOWebScraperDataRequest.url = Data.Url;
                    dTOWebScraperDataRequest.max_pages = ScraperSetting.max_pages;
                    dTOWebScraperDataRequest.Abbreviation = Data.Abbreviation;
                    string APIcrawlseoURL = _configuration["UrlData:APIcrawlseoURL"] ?? string.Empty;
                    var ret1 = await _aPI.GetData(dTOWebScraperDataRequest, APIcrawlseoURL);
                    if (ret1.Data == null)
                        return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ret1.message, ret1.message));
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


        #region Add Server

        public async Task<IActionResult> AddWebServer()
        {
            var allData = await _webServer.GetAll();

            ViewBag.AllData = allData;
            TempData["SuccessMessage"] = null;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWebServer(DTOAddWebServerRequest Request)
        {
            var allData = await _webServer.GetAll();

            ViewBag.AllData = allData;

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
                        return View(Request);
                    }

                    // Keep original Created fields
                    data.Url = Request.Url;
                    data.Includes = Request.Includes;

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