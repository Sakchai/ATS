using ATS.Services;
using ATS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ATS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPersonTrackingService _personTrackingService;
        public HomeController(ILogger<HomeController> logger,
            IPersonTrackingService personTrackingService)
        {
            _logger = logger;
            _personTrackingService = personTrackingService;
        }

        public IActionResult Index(PersonAccessViewModel pModel)
        {
            if (pModel.StartTime.Year == 1)
                pModel.StartTime = DateTime.Today;
            if (pModel.EndTime.Year == 1)
                pModel.EndTime = DateTime.Now;
            pModel.BuildingId = (pModel.BuildingId == 0) ? pModel.BuildingId = 1 : pModel.BuildingId;
            var pTracking = _personTrackingService.GetPersonTrackingByBuilding(pModel.BuildingId, pModel.StartTime, pModel.EndTime);
            pModel.NumberFail = (pTracking != null) ? pTracking.NumberFail : 0;
            pModel.NumberPass = (pTracking != null) ? pTracking.NumberPass : 0;
            pModel.FailedValue = (pTracking != null) ? pTracking.NumberFail.ToString("N0") : "0";
            pModel.PassedValue = (pTracking != null) ? pTracking.NumberPass.ToString("N0") : "0";
            pModel.TotalValue = (pTracking != null) ? pTracking.NumberTotal.ToString("N0") : "0";
            decimal pass = (pTracking != null && pTracking.NumberTotal > 0) ? ((decimal)pTracking.NumberPass * 100)/ (decimal)pTracking.NumberTotal : 0;
            pModel.PercentPass = pass.ToString("N1");

            List<SelectListItem> lists = GetBuildings(pModel.BuildingId);
            pModel.Buildings = lists;
            return View(pModel);
        }

        private List<SelectListItem> GetBuildings(int buildingId)
        {
            var lists = new List<SelectListItem>();
            foreach (var item in _personTrackingService.GetBuildings())
            {
                if (item.Id == buildingId)
                    lists.Add(new SelectListItem { Selected = true, Text = item.Name, Value = item.Id.ToString() });
                else
                    lists.Add(new SelectListItem { Selected = false, Text = item.Name, Value = item.Id.ToString() });
            }

            return lists;
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
