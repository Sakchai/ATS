using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATS.Web.Models
{
    public class PersonAccessViewModel
    {
        public string TotalValue => NumberTotal.ToString("N0");
        public string PassedValue => NumberPass.ToString("N0");
        public string FailedValue => NumberFail.ToString("N0");
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int NumberPass { get; set; }
        public int NumberFail { get; set; }
        public int NumberTotal => NumberPass + NumberFail;
        public string PercentPass => NumberTotal > 0 ? $"{NumberPass * 100 / NumberTotal}" : "";
        public int BuildingId { get; set; }
        public string InvalidLicenseKey { get; set; }
        public List<SelectListItem> Buildings { get; set; }
    }
}
