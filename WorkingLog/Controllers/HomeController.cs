using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.LiteDB.Models;
using LiteDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using WorkingLog.Models;
using FileMode = LiteDB.FileMode;

namespace WorkingLog.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly LiteRepository _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(LiteRepository db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> IndexModel()
        {
            var user = await _userManager.GetUserAsync(User);
            var allitems = _db.Query<WorkingLogItem>().ToList()
                .Where(item=>item.UserId == user.Id);

            var model = new JArray(allitems
                .OrderByDescending(item=>item.Date)
                .ThenBy(item=>item.Project)
                .ThenBy(item=>item.WorkItem)
                .ThenBy(item=>item.Type)
                .Select(item => new JObject
            {
                ["id"] = item.Id.ToString(),
                ["date"] = item.Date.ToString("yy/MM/dd"),
                ["project"] = item.Project,
                ["workItem"] = item.WorkItem,
                ["type"] = item.Type,
                ["hours"] = item.Hours,
                ["detail"] = item.Detail
            })).ToString(Formatting.None);
            
            return Content(model);
        }

        public async Task<IActionResult> Add(DateTime date, string project, string workItem, string type, double hours, string detail)
        {
            var user = await _userManager.GetUserAsync(User);
            
            _db.Insert(new WorkingLogItem
            {
                Id = ObjectId.NewObjectId().ToString(),
                UserId = user.Id,
                Date = date,
                Project = project,
                WorkItem = workItem,
                Type = type,
                Hours = hours,
                Detail = detail
            });
            return Content(new JObject
            {
                ["success"] = true
            }.ToString(Formatting.None));
        }

        public IActionResult Delete(string id)
        {
            _db.Delete<WorkingLogItem>(item => item.Id == id);
            return Content(new JObject
            {
                ["success"] = true
            }.ToString(Formatting.None));
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (file.Length <= 0) return RedirectToAction("Index");
            var newWorkLogs =new List<WorkingLogItem>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var pack = new ExcelPackage(stream))
                {
                    var sheet = pack.Workbook.Worksheets[1];
                    for (var row = 2; row < sheet.Dimension.End.Row; row++)
                    {
                        var date = Convert.ToDateTime(sheet.Cells[row, 1].Value.ToString().Trim());
                        var project = sheet.Cells[row, 2].Value?.ToString().Trim() ?? "";
                        var workItem = sheet.Cells[row, 3].Value?.ToString().Trim() ?? "";
                        var type = sheet.Cells[row, 4].Value?.ToString().Trim() ?? "";
                        var hours = double.Parse(sheet.Cells[row, 5].Value?.ToString().Trim() ?? "0");
                        var detail = sheet.Cells[row, 6].Value?.ToString().Trim() ?? "";
                        newWorkLogs.Add(new WorkingLogItem
                        {
                            Id = ObjectId.NewObjectId().ToString(),
                            Date = date,
                            Project = project,
                            WorkItem = workItem,
                            Type = type,
                            Hours = hours,
                            Detail = detail,
                            UserId = user.Id
                        });
                    }

                }
            }
            _db.Insert<WorkingLogItem>(newWorkLogs);
            return RedirectToAction("Index");
        }



        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
