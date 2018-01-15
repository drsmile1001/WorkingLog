using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WorkingLog.Models;

namespace WorkingLog.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly LiteRepository _db;

        public HomeController(LiteRepository db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IndexModel()
        {

            var allitems = _db.Query<WorkingLogItem>().ToList();

            var model = new JArray(allitems
                .OrderBy(item=>item.Date)
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

        public IActionResult Add(DateTime date, string project, string workItem, string type, double hours, string detail)
        {
            _db.Insert(new WorkingLogItem
            {
                Id = Guid.NewGuid(),
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

        public IActionResult Delete(Guid id)
        {
            _db.Delete<WorkingLogItem>(item => item.Id == id);
            return Content(new JObject
            {
                ["success"] = true
            }.ToString(Formatting.None));
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
