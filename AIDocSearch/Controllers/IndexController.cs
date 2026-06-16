using AIDocSearch.Services;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Domain.Constants;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Domain.Entities;
using Infrastructure.Shared.Services;
using Infrastructure;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public IndexController(
            ISearch searchService,
            IConfiguration configuration,
            IindexUserMapping indexUserMapping,
            IDateTimeService dateTimeService,
            ICurrentUserService currentUserService,
            IUserAccount userAccount)
        {
            _searchService = searchService;
            _configuration = configuration;
            _indexUserMapping = indexUserMapping;
            _dateTimeService = dateTimeService;
            _currentUserService = currentUserService;
            _userAccount = userAccount;
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
            string UserName = _configuration["UrlData:UserName"] ?? string.Empty;
            string Password = _configuration["UrlData:Password"] ?? string.Empty;

            var data = await _searchService.IndexesDetails(Url, UserName, Password);

            return Json(new { success = true, data });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchUsers([FromForm] DTODataTablesRequest request,
    [FromForm] string uuid)
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
        public async Task<IActionResult> GetIndexWiseAssginUsers([FromForm] string IndexId)
        {
            if (_userAccount == null) return Json(new { success = false, data = Array.Empty<object>() });
            var result = await _userAccount.GetIndexWiseAssginUsers(IndexId);
            return Json(new { success = true, data = result });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignIndex([FromBody] DTOIndexAssignRequest request)
        {
            if (!ModelState.IsValid || request == null)
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
                    IndexId = request.IndexId,
                    UserId = uid,
                    CreatedOn = _dateTimeService.NowUtc,
                    UpdatedOn = _dateTimeService.NowUtc,
                    CreatedBy = _currentUserService.UserId,
                    UpdatedBy = _currentUserService.UserId,
                    IsActive = true,
                    IsDeleted = false
                };
                await _indexUserMapping.AddWithCheckIndexId(map);
            }
           

            return Json(new DTOGenericResponse<object>(
                ConnKeyConstants.Success,
                ConnKeyConstants.SuccessMessage,
                null));
        }

    }
}
