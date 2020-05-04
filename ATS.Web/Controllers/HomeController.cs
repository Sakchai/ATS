using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ATS.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ATS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(PersonAccessViewModel persons)
        {
            if (persons.TotalValue == null)
            {
                persons.Failed = 147;
                persons.Passed = 1607;
                persons.FailedValue = "147";
                persons.PassedValue = "1,607";
                persons.TotalValue = "1,754";
                persons.PercentPass = "91.6";
                persons.StartTime = DateTime.Today;
                persons.EndTime = DateTime.Now;
                persons.BuildingId = 1;
                var lists = new List<SelectListItem>();
                lists.Add(new SelectListItem { Selected = true, Text = "Building A", Value = "1" });
                lists.Add(new SelectListItem { Selected = false, Text = "Building B", Value = "2" });
                persons.Buildings = lists;
            } 
            return View(persons);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
