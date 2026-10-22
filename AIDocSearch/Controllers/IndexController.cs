using AIDocSearch.Helpers;
using AIDocSearch.Services;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Domain.CommonModel;
using Domain.Constants;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Shared.Helpers;
using Infrastructure.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.AccessControl;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AIDocSearch.Controllers
{
    [Authorize]
    public class IndexController : Controller
    {
        private readonly ISearch _searchService;
        private readonly IConfiguration _configuration;
        private readonly IindexUserMapping _indexUserMapping;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserAccount _userAccount;
        private readonly IAESEncrytDecry _AESEncrytDecry;
        public IndexController(
            ISearch searchService,
            IConfiguration configuration,
            IindexUserMapping indexUserMapping,
            IDateTimeService dateTimeService,
            ICurrentUserService currentUserService, IAESEncrytDecry aESEncrytDecry,
            IUserAccount userAccount)
        {
            _searchService = searchService;
            _configuration = configuration;
            _indexUserMapping = indexUserMapping;
            _dateTimeService = dateTimeService;
            _currentUserService = currentUserService;
            _userAccount = userAccount;
            _AESEncrytDecry = aESEncrytDecry;
        }

        // returns the view that will load data client-side
        [HttpGet]
        public IActionResult IndexesDetails()
        {
            return View();
        }

        // API endpoint used by client-side JS (POST + antiforgery)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetIndexesDetails()
        {
            string Url = _configuration["UrlData:indicesUrl"] ?? string.Empty;
            string encodedKey = SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").encodedKey;
            var data = await _searchService.IndexesDetails(Url, encodedKey);
           
            if ((string)_currentUserService.RoleName =="User")
            {
                var userIndexes = await _userAccount.GetAllIndexUserWise(_currentUserService.UserId);
                var allowedIndexIds = userIndexes
     .Select(x => x.IndexId)
     .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var filteredData = data
                    .Where(i => allowedIndexIds.Contains(i.uuid))
                    .ToList();
                return Json(new { success = true, data = filteredData,type = (string)_currentUserService.RoleName });
            }
            else
            {
                return Json(new { success = true, data = data.Where(i=>i.index.Contains("fs-")).ToList(), type = (string)_currentUserService.RoleName });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchUsers([FromForm] DTODataTablesRequest request, [FromForm] string uuid)
        {
            // Basic datatables-like request mapping
            //var request = new DTODataTablesRequest
            //{
            //    searchValue = q ?? string.Empty,
            //    Start = 0,
            //    Length = 20,
            //    Draw = 1
            //};

            if (_userAccount == null) return Json(new { success = false, data = Array.Empty<object>() });

            var result = await _userAccount.GetActive_ForIndex_Mapping_Users(request, uuid);

            return Json(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetIndexWiseAssginUsers([FromForm] string IndexId)
        {
            if (_userAccount == null) return Json(new { success = false, data = Array.Empty<object>() });
            var result = await _userAccount.GetIndexWiseAssginUsers(IndexId, (int)_currentUserService.UserId);
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignIndex([FromBody] EncryptedRequest Payload)
        {
            DTOIndexAssignRequest request = await _AESEncrytDecry.DecryptAESWithDTO<DTOIndexAssignRequest>(Payload.Data, SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").AESKey);
            if (request == null)
                return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ConnKeyConstants.BadRequestMessage, null));

            if (!TryValidateModel(request))
                return BadRequest(new { success = false, message = "Invalid request" });

            await _indexUserMapping.UserAssginAndDeAssignIndex(request);
            if (request.AllSelected)
            {
                    var allUsers = await _userAccount.GetActiveUsers();
                    request.UserIds = allUsers.Select(u => u.Id).ToList();
            }
            // create mappings
            foreach (var uid in request.UserIds)
            {
                var map = new TrnIndexUserMapping
                {
                    IndexName=request.indexName,
                    IndexId = request.IndexId,
                    UserId = uid,
                   
                };
                await _indexUserMapping.AddWithCheckIndexId(map);
            }
           

            return Json(new DTOGenericResponse<object>(
                ConnKeyConstants.Success,
                ConnKeyConstants.SuccessMessage,
                null));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetDocDetailsByIndexName(string indexName,string uuid, int OcrType)
        {
           string AESKey= SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").AESKey;
             indexName = _AESEncrytDecry.DecryptAES(indexName, AESKey);
             uuid = _AESEncrytDecry.DecryptAES(uuid, AESKey);

            if(string.IsNullOrEmpty(indexName) || string.IsNullOrEmpty(uuid))
                return Json(new { success = false });

            int CurrentUserId = (int)_currentUserService.UserId;

            if ((string)_currentUserService.RoleName == "User")
            {
                var userIndexes = await _userAccount.GetAllIndexUserWise(_currentUserService.UserId);

                var isAllowed = userIndexes.Any(x =>
                    string.Equals(x.IndexName, indexName, StringComparison.OrdinalIgnoreCase));

                if (!isAllowed)
                    return Forbid();

                // Check connection only after confirming the user has access.
            }



            bool ret = await _indexUserMapping.CheckUserIndexingExists(CurrentUserId, uuid, (string)_currentUserService.RoleName);
            if(ret)
            {
                string Url = _configuration["UrlData:BaseUrl"] ?? string.Empty;
                string encodedKey = SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").encodedKey;

                var retindex = await _searchService.GetDocDetailsByIndexName(Url, indexName, OcrType,encodedKey);

                List<FileSource> data = new();

                foreach (var item in retindex)
                {
                    FileSource fileData = new FileSource
                    {
                        File = new FileInfoData
                        {
                            FileName = item.File?.FileName,
                            Extension = item.File?.Extension,
                            FileSize = item.File?.FileSize ?? 0
                        },

                        Path = new PathData(),

                        url = item.url,
                        title= item.title
                    };


                    if (item.Path != null && item.Path.VirtualPath!=null)
                    {
                        fileData.Path.Real =
                            _AESEncrytDecry.EncryptAES(
                                item.Path.VirtualPath,
                                AESKey
                            );
                    }
                    fileData.crawler = item.crawler;

                    data.Add(fileData);
                }
                    //fileSource.Path = lstfile;
                    return Json(new { success = true, data });
            }
            else
            {
                return Json(new { success = false });

            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetstatisticsByIndexName(string indexName, string uuid)
        {
            string AESKey = SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").AESKey;
            indexName = _AESEncrytDecry.DecryptAES(indexName, AESKey);
            uuid = _AESEncrytDecry.DecryptAES(uuid, AESKey);

            if (string.IsNullOrEmpty(indexName) || string.IsNullOrEmpty(uuid))
                return Json(new { success = false });

            int CurrentUserId = (int)_currentUserService.UserId;

            bool ret = await _indexUserMapping.CheckUserIndexingExists(CurrentUserId, uuid, (string)_currentUserService.RoleName);
            if (ret)
            {
                string Url = _configuration["UrlData:BaseUrl"] ?? string.Empty;
                string encodedKey = SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users").encodedKey;

                var retindex = await _searchService.GetDocCountByIndexName(Url, indexName, encodedKey);
                    
                var retstatistics = await _searchService.GetstatisticsByIndexName(Url, indexName, encodedKey);

                var data = new DTODocCountByIndexNameWithStatisticResponse
                {
                    DTODocCountByIndexName = retindex,
                    DTOStatisticsHits = retstatistics,
                   
                    
                };
                //fileSource.Path = lstfile;
                return Json(new { success = true, data });
            }
            else
            {
                return Json(new { success = false });

            }

        }
    }
}
