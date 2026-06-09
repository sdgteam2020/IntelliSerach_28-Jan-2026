using AIDocSearch.Helpers;
using BusinessLogicsLayer.AddWebServer;
using BusinessLogicsLayer.Helpers;
using BusinessLogicsLayer.ScraperSetting;
using BusinessLogicsLayer.UnitOfWorks;
using DataTransferObject.CommonModel;
using DataTransferObject.Constants;
using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Pdf.Xobject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIDocSearch.Controllers
{
    public class MasterController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment _env;
        private readonly IWebScraperSetting _webScraperSetting;
        private readonly IWebServer _webServer;

        public MasterController(IUnitOfWork unitOfWork, IWebHostEnvironment env, IWebScraperSetting webScraperSetting, IWebServer webServer)
        {
            this.unitOfWork = unitOfWork;
            _env = env;
            _webScraperSetting = webScraperSetting;
            _webServer = webServer;
        }

        #region Master Table

        [AllowAnonymous]
        public async Task<IActionResult> GetAllMMaster(DTOMasterRequest Data)
        {
            try
            {
                var ret = await unitOfWork.GetAllMMaster(Data);
                var response = new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessMessage, ret);
                return Json(response);
            }
            catch
            {
                var response = new DTOGenericResponse<object>(ConnKeyConstants.InternalServerError, ConnKeyConstants.InternalServerErrorMessage, "Null");
                return Json(response);
            }
        }

        #endregion Master Table

        [HttpGet]
        public IActionResult WatermarkPdfWithFolder(string fileName, string? folderPath)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("fileName is required");

            // Default folder
            folderPath = string.IsNullOrWhiteSpace(folderPath)
                ? "UploadForIndexing"
                : folderPath;

            // Physical path
            string folderPhysical = System.IO.Path.Combine(_env.WebRootPath, folderPath);
            string inputPath = System.IO.Path.Combine(folderPhysical, fileName);

            if (!System.IO.File.Exists(inputPath))
                return NotFound("File not found");

            // Output filename (same PDF name – no overwrite on disk)
            string outFileName = System.IO.Path.GetFileName(fileName);

            // ===== CLIENT IP HANDLING (SAFE) =====
            var connection = HttpContext?.Connection;
            if (connection == null)
            {
                // background task / non-HTTP context / edge case
                return NotFound("IP Not Found");
            }

            string clientIP = connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var forwardedHeader = Request?.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(forwardedHeader))
            {
                clientIP = forwardedHeader
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(ip => ip.Trim())
                    .FirstOrDefault()
                    ?? clientIP;
            }


            // DateTime stamp
            string dateTimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Output stream
            using var outputStream = new MemoryStream();

            // ===== PDF PROCESSING =====
            using (var reader = new PdfReader(inputPath))
            using (var writer = new PdfWriter(outputStream))
            using (var pdf = new PdfDocument(reader, writer))
            {
                PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                for (int p = 1; p <= pdf.GetNumberOfPages(); p++)
                {
                    var page = pdf.GetPage(p);
                    var rect = page.GetPageSize();

                    // Stamp annotation (full page)
                    var stamp = new PdfStampAnnotation(rect);
                    stamp.SetFlags(PdfAnnotation.PRINT);

                    // Appearance XObject
                    var appearance = new PdfFormXObject(rect);
                    var canvas = new PdfCanvas(appearance, pdf);

                    // Transparency
                    var gs = new PdfExtGState().SetFillOpacity(0.20f);

                    canvas.SaveState();
                    canvas.SetExtGState(gs);
                    canvas.SetFillColor(new DeviceRgb(255, 0, 0));

                    canvas.BeginText();
                    canvas.SetFontAndSize(font, 50);

                    float angle = (float)(Math.PI / 4); // 45 degrees
                    float gap = 80;

                    string[] lines = { clientIP, dateTimeNow };

                    float startX = rect.GetWidth() * 0.20f;
                    float startY = rect.GetHeight() * 0.25f;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        float dx = (float)Math.Sin(angle) * gap * i;
                        float dy = -(float)Math.Cos(angle) * gap * i;

                        canvas.SetTextMatrix(
                            (float)Math.Cos(angle), (float)Math.Sin(angle),
                            (float)-Math.Sin(angle), (float)Math.Cos(angle),
                            startX + dx, startY + dy
                        );

                        canvas.ShowText(lines[i]);
                    }

                    canvas.EndText();
                    canvas.RestoreState();

                    // Attach appearance
                    stamp.SetNormalAppearance(appearance.GetPdfObject());
                    page.AddAnnotation(stamp);
                }
            }

            // ===== INLINE PDF VIEW =====
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{outFileName}\"";
            return File(outputStream.ToArray(), "application/pdf");
        }

        #region Add WebScraperSetting

        public async Task<IActionResult> AddWebScraperSetting()
        {
            var allData = await _webScraperSetting.GetWebScraperSetting();

            ViewBag.AllData = allData;
            TempData["SuccessMessage"] = null;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWebScraperSetting(DTOWebScraperSettingRequest Request)
        {
            var allData = await _webScraperSetting.GetWebScraperSetting();

            ViewBag.AllData = allData;

            if (ModelState.IsValid)
            {
                var UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var now = DateTime.UtcNow;
                WebScraperSetting data;

                if (Request.Id > 0)
                {
                    // 🔹 UPDATE
                    data = await _webScraperSetting.GetById(Convert.ToInt32(Request.Id)); // fetch existing record

                    if (data == null)
                    {
                        ModelState.AddModelError(string.Empty, "Record not found.");
                        return View(Request);
                    }

                    // Keep original Created fields
                    data.max_pages = Request.max_pages;
                    data.max_pdfs = Request.max_pdfs;
                    data.UpdatedOn = now;
                    data.UpdatedBy = UserId;
                }
                else
                {
                    // 🔹 INSERT
                    data = new WebScraperSetting
                    {
                        max_pages = Request.max_pages,
                        max_pdfs = Request.max_pdfs,
                        CreatedOn = now,
                        UpdatedOn = now,
                        CreatedBy = UserId,
                        UpdatedBy = UserId,
                        IsActive = true,
                        IsDeleted = false
                    };
                }

                if (Request.Id > 0)
                {
                    WebScraperSetting ret = await _webScraperSetting.UpdateWebScraperSetting(data);
                    if (ret != null && ret.Id > 0)
                    {
                        TempData["SuccessMessage"] = "Record Update successfully.";
                        return View(Request);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ret.max_pages.ToString() ?? "Record Not Save.");
                        return View(data);
                    }
                }
                else
                {
                    TempData["SuccessMessage"] = "Not Allow Only Update Allow.";
                    return View(Request);
                }
            }

            return View(Request);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetWebScraperSetting()
        {
            return Json(await _webScraperSetting.GetWebScraperSetting());
        }
        #endregion


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
        public async Task<IActionResult> AddWebServer(string EncryptedData)
        {
            DTOAddWebServerRequest Request = await AESEncrytDecry.DecryptAESWithDTO<DTOAddWebServerRequest>(EncryptedData, SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").AESKey);

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
            ModelState.Clear();
            if (TryValidateModel(Request))
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
                    data.Url = Request.Url.Trim();
                    data.Includes = Request.Includes.Trim();
                    data.Index_Name = Request.Index_Name.Trim();
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
                    
                    ViewBag.AllData = await _webServer.GetAll();
                    TempData["SuccessMessage"] = "Record Save successfully.";
                    return View(Request);
                }
                else
                {
                    ViewBag.AllData = await _webServer.GetAll();
                    ModelState.AddModelError(string.Empty, ret.Url ?? "Record Not Save.");
                    TempData["ErrorMessage"] = $"Record Not Save.";
                    return View(data);
                }
            }


            ViewBag.AllData = await _webServer.GetAll();
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