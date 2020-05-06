using ATS.Model;
using ATS.Services;
using Autofac;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ATS.Scheduler
{
    public class PersonTracker
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IContainer _container;
        private IPersonTrackingService _personTrackingService;
        public PersonTracker(IContainer container, IPersonTrackingService personTrackingService)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            _container = container;
            _personTrackingService = personTrackingService;
        }

        public void Create()
        {
            try
            {
                string sourceDirectory = ConfigurationManager.AppSettings["currentPath"];
                string archiveDirectory = ConfigurationManager.AppSettings["archivePath"];
                int buildingId = Int32.Parse(ConfigurationManager.AppSettings["buildingId"]);

                var txtFiles = Directory.EnumerateFiles(sourceDirectory, "*.txt", SearchOption.AllDirectories);

                foreach (string currentFile in txtFiles)
                {
                    CreateOrUpdatePersonAccess(buildingId, currentFile);

                    string fileName = currentFile.Substring(sourceDirectory.Length + 1);
                    if (!Directory.Exists(archiveDirectory))
                        Directory.CreateDirectory(archiveDirectory);
                    Directory.Move(currentFile, Path.Combine(archiveDirectory, fileName));
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        private void CreateOrUpdatePersonAccess(int buildingId, string currentFile)
        {
            using (StreamReader file = new StreamReader(currentFile))
            {
                int pass = Int32.Parse(file.ReadLine());
                file.ReadLine();
                int failed = Int32.Parse(file.ReadLine());
                var tranDate = GetTranDate(currentFile);
                var personTran = _personTrackingService.GetPersonTrackingByTranDate(buildingId, tranDate);

                if (personTran != null)
                    UpdatePersonAccess(buildingId, pass, failed, personTran);
                else
                    InsertPersonAccess(buildingId, pass, failed, tranDate);
                file.Close();
            }
        }

        private void UpdatePersonAccess(int buildingId, int pass, int failed, PersonAccess personTran)
        {
            personTran.NumberFail = failed;
            personTran.NumberPass = pass;
            personTran.NumberTotal = failed + pass;
            _personTrackingService.UpdatePersonTracking(personTran);
            log.Info($"Update BuildingId:{buildingId},NumberPass:{pass},NumberFail:{failed},TranDate:{personTran.TranDate.ToShortTimeString()}");
        }

        private void InsertPersonAccess(int buildingId, int pass, int failed, DateTime tranDate)
        {
            var person = new PersonAccess
            {
                BuildingId = buildingId,
                NumberFail = failed,
                NumberPass = pass,
                NumberTotal = pass + failed,
                TranDate = tranDate
            };

            log.Info($"Insert BuildingId:{buildingId},NumberPass:{pass},NumberFail:{failed},TranDate:{person.TranDate.ToShortTimeString()}");
            _personTrackingService.InsertPersonTracking(person);
        }

        private DateTime GetTranDate(string currentFile)
        {
            string dateString = Path.GetFileNameWithoutExtension(currentFile);
            
            CultureInfo provider = CultureInfo.InvariantCulture;
            // It throws Argument null exception  
            return DateTime.ParseExact(dateString, "yyyyMMddHHmmss", provider);
        }

    }
}
