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
using Infrastructure;
using Infrastructure.Shared.Helpers;
using Infrastructure.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
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
        private readonly ConnectionManager _cm;
        private readonly IHubContext<CrawlerHub> _hub;
        public IndexController(
            ISearch searchService,
            IConfiguration configuration,
            IindexUserMapping indexUserMapping,
            IDateTimeService dateTimeService,
            ICurrentUserService currentUserService, IAESEncrytDecry aESEncrytDecry,
            IUserAccount userAccount,
            ConnectionManager cm, IHubContext<CrawlerHub> hub)
        {
            _searchService = searchService;
            _configuration = configuration;
            _indexUserMapping = indexUserMapping;
            _dateTimeService = dateTimeService;
            _currentUserService = currentUserService;
            _userAccount = userAccount;
            _AESEncrytDecry = aESEncrytDecry;
            _cm = cm;
            _hub = hub;
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
            string Url = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__indicesUrl") ?? string.Empty;
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

            bool isPrivate = await _indexUserMapping.IsPrivateIndex(request.IndexId);
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
                    IsPrivate = isPrivate   

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
                string Url = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__BaseUrl") ?? string.Empty;
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
                string Url = Environment.GetEnvironmentVariable("IntelliSearch__UrlData__BaseUrl") ?? string.Empty;
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestartCrawler([FromBody] DTOIndexerByCrawlerId data)
        {
            if(string.IsNullOrEmpty(data.CrawlerId))
            {
                return BadRequest(new
                {
                    Code = 400,
                    Message = "CrawlerId Not Null"
                });
            }

            if (!_cm.TryGetConnectionId(data.CrawlerId, out var connectionId))
            {
                return BadRequest(new
                {
                    Code = 400,
                    Message = "Indexer not connected"
                });
            }


            var (requestId, alreadyExists) =
                _cm.CreateRequest(data.CrawlerId);

            if (!alreadyExists)
                await _hub.Clients.Client(connectionId)
                    .SendAsync("RestartIndexer", requestId);

            return Ok(new
            {
                Code = 200,
                Message = "Indexer restarted"
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> UpdateIndexPrivatePublic([FromBody] DTOIndexPublicPrivateRequest data)
        {
            int CurrentUserId = (int)_currentUserService.UserId;
            string RoleName = (string)_currentUserService.RoleName;

            if (data == null ||
                string.IsNullOrEmpty(data.CrawlerId) ||
                string.IsNullOrEmpty(data.uuid))
            {
                return BadRequest(new
                {
                    Code = 400,
                    Message = "Invalid request data"
                });
            }

            string successMessage = data.IsPrivate
                ? "Index successfully changed to Private."
                : "Index successfully changed to Public.";

            string failureMessage = data.IsPrivate
                ? "Failed to make index Private."
                : "Failed to make index Public.";

            bool ret = await _indexUserMapping
                .CheckUserIndexingExists(CurrentUserId, data.uuid);

            if (!ret)
            {
                var map = new TrnIndexUserMapping
                {
                    IndexName = data.CrawlerId,
                    IndexId = data.uuid,
                    UserId = CurrentUserId,
                    IsPrivate = data.IsPrivate
                };

                await _indexUserMapping.AddWithCheckIndexId(map);

                return Ok(new
                {
                    Code = 200,
                    Message = successMessage
                });
            }

            bool result = await _indexUserMapping.UpdateIndexPrivatePublic(
                data.CrawlerId,
                data.IsPrivate,
                CurrentUserId.ToString(),
                RoleName
            );

            if (!result)
            {
                return BadRequest(new
                {
                    Code = 400,
                    Message = failureMessage
                });
            }

            return Ok(new
            {
                Code = 200,
                Message = successMessage
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAllPrivateIndexes()
        {
            int CurrentUserId = (int)_currentUserService.UserId;
            string RoleName = (string)_currentUserService.RoleName;
            var privateIndexes = await _indexUserMapping.GetAllPrivateIndex(RoleName, CurrentUserId.ToString());
            return Ok(new
            {
                Code = 200,
                Message = "Private indexes retrieved successfully",
                Data = privateIndexes
            });
        }
    }
}
