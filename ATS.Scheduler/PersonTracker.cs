using ATS.Model;
using ATS.Services;
using Autofac;
using log4net;
using log4net.Config;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ATS.Dto;
using Newtonsoft.Json;

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

        public void  Create()
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
                    string desFile = Path.Combine(archiveDirectory, fileName);
                    if (File.Exists(desFile))
                        File.Delete(desFile);
                    Directory.Move(currentFile, desFile);
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
                int total = Int32.Parse(file.ReadLine());
                file.ReadLine();
                int failed = Int32.Parse(file.ReadLine());
                var tranDate = GetTranDate(currentFile);

                var personTran = _personTrackingService.GetPersonTrackingByTranDate(buildingId, tranDate);

                if (personTran != null)
                    UpdatePersonAccess(buildingId, total, failed, personTran);
                else
                    InsertPersonAccess(buildingId, total, failed, tranDate);
                file.Close();
            }
        }

        private void UpdatePersonAccess(int buildingId, int total, int failed, PersonAccess personTran)
        {
            personTran.NumberFail = failed;
            personTran.NumberPass = total - failed;
            personTran.NumberTotal = total;
            _personTrackingService.UpdatePersonTracking(personTran);
            log.Info($"Update BuildingId:{buildingId},NumberPass:{total - failed},NumberFail:{failed},TranDate:{personTran.TranDate.ToShortTimeString()}");
        }

        private void InsertPersonAccess(int buildingId, int total, int failed, DateTime tranDate)
        {
            var lastPerson = _personTrackingService.GetLastPersonTracking(buildingId);

            int remainFail;
            int remainPass;
            int remainTotal;
            if (lastPerson != null)
            {
                if (total > lastPerson.NumberTotal)
                {
                    remainTotal = total - lastPerson.NumberTotal;
                    remainFail = failed - lastPerson.NumberPass;
                    remainPass = remainTotal - remainFail;
                }
                else
                {
                    remainTotal = total;
                    remainFail = failed;
                    remainPass = remainTotal - remainFail;
                }
            }
            else
            {
                remainPass = total - failed;
                remainFail = failed;
            }

            var person = new PersonAccess
            {
                BuildingId = buildingId,
                NumberFail = failed,
                NumberPass = total - failed,
                NumberTotal = total,
                RemainFail = remainFail,
                RemainPass = remainPass,
                TranDate = tranDate
            };

            log.Info($"Insert BuildingId:{buildingId},NumberPass:{total - failed},NumberFail:{failed},TranDate:{person.TranDate.ToShortTimeString()}");
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
