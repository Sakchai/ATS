using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATS.Web.Models
{
    public class PersonAccessViewModel
    {
        public string TotalValue { get; set; }
        public string PassedValue { get; set; }
        public string FailedValue { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public string PercentPass { get; set; }
        public int BuildingId { get; set; }
        public List<SelectListItem> Buildings { get; set; }
    }
}
