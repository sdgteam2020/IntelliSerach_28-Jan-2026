using BusinessLogicsLayer.ScraperAPI;
using DataTransferObject.Constants;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDocSearch.Controllers
{
    [Authorize] // Require authentication for all actions in this controller
    public class ScraperController : Controller
    {
        private readonly IAPI _aPI;
        public ScraperController(IAPI aPI)
        {
            _aPI = aPI;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Scrapering(DTOScraperRequest Data)
        {
            DTOAPILoginRequest dTOAPILoginRequest = new DTOAPILoginRequest();
            dTOAPILoginRequest.username= "admin";
            dTOAPILoginRequest.password= "Admin@123";

            var ret= await _aPI.Getauthentication(dTOAPILoginRequest);
            if (ret != null)
            {
                DTOScraperDataRequest dTOScraperDataRequest = new DTOScraperDataRequest();
                dTOScraperDataRequest.CSRFToken = ret.CSRFToken;
                dTOScraperDataRequest.session_key = ret.session_key;
                dTOScraperDataRequest.url = Data.Url;
                dTOScraperDataRequest.max_pdfs = 500;
                var ret1 = await _aPI.GetData(dTOScraperDataRequest);
                return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.ScraperingMessage, ret1));
            }
            else
            {
                return Json(new DTOGenericResponse<object>(ConnKeyConstants.InternalServerError, ConnKeyConstants.InternalServerErrorMessage, null));
            }
           
        }
    }
}
