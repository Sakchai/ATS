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
    public class PersonTrackerAPI
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private HttpClient client = new HttpClient();
        public PersonTrackerAPI()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

        }

        public async Task CreateAsync()
        {
            try
            {
                string sourceDirectory = ConfigurationManager.AppSettings["currentPath"];
                string archiveDirectory = ConfigurationManager.AppSettings["archivePath"];
                int buildingId = Int32.Parse(ConfigurationManager.AppSettings["buildingId"]);
                var txtFiles = Directory.EnumerateFiles(sourceDirectory, "*.txt", SearchOption.AllDirectories);

                foreach (string currentFile in txtFiles)
                {

                    await CreateOrUpdatePersonAccessAPI(buildingId, currentFile);

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
        private async Task CreateOrUpdatePersonAccessAPI(int buildingId, string currentFile)
        {
            using (StreamReader file = new StreamReader(currentFile))
            {
                string tranDate = Path.GetFileNameWithoutExtension(currentFile);

                int total = Int32.Parse(file.ReadLine());
                file.ReadLine();
                int failed = Int32.Parse(file.ReadLine());

                var personTran = await GetPersonTrackingByTranDate(buildingId, tranDate);

                if (personTran != null)
                {
                    personTran.NumberFail = failed;
                    personTran.NumberTotal = total;
                    await UpdatePersonAccessAsync(personTran);
                }
                else
                {
                    var personAccessDto = new PersonAccessDto
                    {
                        BuildingId = buildingId,
                        Total = total,
                        Failed = failed,
                        TranDate = GetTranDate(currentFile)
                    };
                    await CreatePersonAccessAsync(personAccessDto);
                }
                file.Close();
            }
        }

        private async Task<Uri> CreatePersonAccessAsync(PersonAccessDto p)
        {
            string uri = $"{ConfigurationManager.AppSettings["BaseAddress"]}/api/PersonTracking";
            HttpResponseMessage response = await client
                .PostAsync(uri, new StringContent(JsonConvert.SerializeObject(p), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        private async Task UpdatePersonAccessAsync(PersonAccess p)
        {
            string uri = $"{ConfigurationManager.AppSettings["BaseAddress"]}/api/PersonTracking";
            HttpResponseMessage response = await client
                .PutAsync(uri, new StringContent(JsonConvert.SerializeObject(p), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();


        }

        private async Task<PersonAccess> GetPersonTrackingByTranDate(int buildingId, string tranDate)
        {
            string path = $"{ConfigurationManager.AppSettings["BaseAddress"]}/api/PersonTracking/{buildingId}/{tranDate}";
            PersonAccess person = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode && !response.ReasonPhrase.Equals("No Content"))
                person = await response.Content.ReadAsAsync<PersonAccess>();

            return person;
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
