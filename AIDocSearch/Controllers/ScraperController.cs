using BusinessLogicsLayer.ScraperAPI;
using DataTransferObject.Constants;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AIDocSearch.Controllers
{
    [Authorize] // Require authentication for all actions in this controller
    public class ScraperController : Controller
    {
        private readonly IAPI _aPI; private readonly IConfiguration _configuration;

        public ScraperController(IAPI aPI, IConfiguration configuration)
        {
            _aPI = aPI;
            _configuration = configuration;
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
                    dTOScraperDataRequest.max_pdfs = 500;
                    dTOScraperDataRequest.Abbreviation = Data.Abbreviation;

                    string APIcrawlURL = _configuration["UrlData:APIcrawlURL"] ?? string.Empty;
                    var ret1 = await _aPI.GetData(dTOScraperDataRequest, APIcrawlURL);
                    if(ret1.data==null)
                        return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ret1.message, ret1.message));
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.ScraperingMessage, ret1));
                }
                else if (ret != null && !Data.IsPdf && ret.Status == true)
                {
                    DTOWebScraperDataRequest dTOWebScraperDataRequest = new DTOWebScraperDataRequest();
                    dTOWebScraperDataRequest.CSRFToken = ret.CSRFToken;
                    dTOWebScraperDataRequest.session_key = ret.session_key;
                    dTOWebScraperDataRequest.url = Data.Url;
                    dTOWebScraperDataRequest.max_pages = 500;
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
    }
}