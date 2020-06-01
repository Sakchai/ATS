using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ATS.Dto;
using ATS.Model;
using ATS.Services;
using ATS.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ATS.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonTrackingController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPersonTrackingService _personTrackingService;
        public PersonTrackingController (ILogger<HomeController> logger,
            IPersonTrackingService personTrackingService)
        {
            _logger = logger;
            _personTrackingService = personTrackingService;
        }

        [HttpGet("{buildingId}/{tranDate}")]
        public ActionResult<PersonAccess> Get(int buildingId, string tranDate)
        {
            if (buildingId == 0)
                return BadRequest();
            if (string.IsNullOrWhiteSpace(tranDate))
                return BadRequest();

            var _tranDate = GetTranDate(tranDate);
            return _personTrackingService.GetPersonTrackingByTranDate(buildingId, _tranDate);
        }

        private DateTime GetTranDate(string dateValue)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            // It throws Argument null exception  
            return DateTime.ParseExact(dateValue, "yyyyMMddHHmmss", provider);
        }

        // POST: api/PersonTracking
        [HttpPost]
        public IActionResult Post(PersonAccessDto p)
        {
            if ((p.BuildingId == 0) || (p.TranDate.Year == 1))
            {
                return BadRequest();
            }

            var lastPerson = _personTrackingService.GetLastPersonTracking(p.BuildingId);

            var person = new PersonAccess
            {
                BuildingId = p.BuildingId,
                NumberFail = p.Failed,
                NumberPass = p.Total - p.Failed,
                NumberTotal = p.Total,
                RemainFail = p.Failed,
                RemainPass = p.Total - p.Failed,
                TranDate = p.TranDate
            };

            if ((lastPerson != null) && (lastPerson.Equals(person)))
                _logger.LogWarning($"Equal previous record BuildingId:{p.BuildingId},NumberPass:{p.Total - p.Failed},NumberFail:{p.Failed},TranDate:{person.TranDate.ToLongDateString()}");
            else
            {
                _logger.LogInformation($"Insert BuildingId:{p.BuildingId},NumberPass:{p.Total - p.Failed},NumberFail:{p.Failed},TranDate:{person.TranDate.ToLongDateString()}");
                _personTrackingService.InsertPersonTracking(person);
            }
            return NoContent();
        }

        // PUT: api/PersonTracking/5
        [HttpPut]
        public ActionResult<PersonAccess> Put(PersonAccess p)
        {
            if ((p.BuildingId == 0) || (p.NumberTotal == 0) || (p.TranDate.Year == 1))
            {
                return BadRequest();
            }
            var p1 = _personTrackingService.GetPersonTrackingByTranDate(p.BuildingId.Value, p.TranDate);
            if (p1 == null)
                _logger.LogInformation($"Update Not found BuildingId:{p.BuildingId},TranDate:{p.TranDate.ToLongDateString()}");

            _personTrackingService.UpdatePersonTracking(p);
            _logger.LogInformation($"Update BuildingId:{p.BuildingId},NumberPass:{p.NumberTotal - p.NumberFail},NumberFail:{p.NumberFail},TranDate:{p.TranDate.ToLongDateString()}");

            return p;
        }


    }
}
