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
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace ATS.Scheduler
{
    public class PersonTrackerFile
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private HttpClient client = new HttpClient();
        public PersonTrackerFile()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

        }

        public async Task Create()
        {
            try
            {
                string sourceDirectory = ConfigurationManager.AppSettings["currentPath"];
                string archiveDirectory = ConfigurationManager.AppSettings["archivePath"];
                int buildingId = Int32.Parse(ConfigurationManager.AppSettings["buildingId"]);
                var txtFiles = Directory.EnumerateFiles(sourceDirectory, "*.PNG", SearchOption.AllDirectories);
                int passed = 0;
                int failed = 0;
                string creationDate = string.Empty;
                string startFile = string.Empty;
                string endFile = string.Empty;
                int i = 0;
                float passedCC = float.Parse(ConfigurationManager.AppSettings["PassedCC"]);
                foreach (string currentFile in txtFiles)
                {
                    string fileName = currentFile.Substring(sourceDirectory.Length + 1);
                    startFile = (i == 0) ? Path.GetFileName(currentFile) : string.Empty;
                    endFile = Path.GetFileName(currentFile);
                    string[] data = Path.GetFileNameWithoutExtension(currentFile).Split('_');
                    creationDate = $"{data[0]}{data[1]}";
                    if (float.Parse(data[2]) >= passedCC)
                        passed++;
                    else
                        failed++;
                    if (!Directory.Exists(archiveDirectory))
                        Directory.CreateDirectory(archiveDirectory);
                    string desFile = Path.Combine(archiveDirectory, fileName);
                    if (File.Exists(desFile))
                        File.Delete(desFile);
                    Directory.Move(currentFile, desFile);
                    i++;
                }
                log.Info($"Start File:{startFile},End File:{endFile},No. of Passed: {passed}, No. of Failed: {failed}, Tran Date:{creationDate}.");
                await CreateOrUpdatePersonAccessFileV3(buildingId, passed, failed, creationDate);
                
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        private async Task CreateOrUpdatePersonAccessFileV3(int buildingId, int passed, int failed, string creationDate)
        {

            var personTran = await GetPersonTrackingByTranDate(buildingId, creationDate);

            if (personTran != null)
            {
                personTran.NumberFail = failed;
                personTran.NumberTotal = passed + failed;
                await UpdatePersonAccessAsync(personTran);
            }
            else
            {
                var personAccessDto = new PersonAccessDto
                {
                    BuildingId = buildingId,
                    Failed = failed,
                    Total = failed+ passed,
                    TranDate = GetTranDate(creationDate)
                };
                await CreatePersonAccessAsync(personAccessDto);
            }

        }

        public IEnumerable<string> GetFiles(string path, string searchPatterns, SearchOption searchOptions = SearchOption.TopDirectoryOnly)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            var dirs = (from file in dir.EnumerateFiles(searchPatterns, searchOptions)
                        orderby file.CreationTime ascending
                        select file.FullName).Distinct(); // Don't need <string> here, since it's implied
            return dirs;
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

        private async Task<PersonAccess> UpdatePersonAccessAsync(PersonAccess p)
        {
            string uri = $"{ConfigurationManager.AppSettings["BaseAddress"]}/api/PersonTracking";
            HttpResponseMessage response = await client
                .PutAsync(uri, new StringContent(JsonConvert.SerializeObject(p), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            PersonAccess person = null;
            if (response.IsSuccessStatusCode && !response.ReasonPhrase.Equals("No Content"))
            {
                string json = await response.Content.ReadAsStringAsync();
                person = JsonConvert.DeserializeObject<PersonAccess>(json);
            }
            return person;
        }

        private async Task<PersonAccess> GetPersonTrackingByTranDate(int buildingId, string tranDate)
        {
            string path = $"{ConfigurationManager.AppSettings["BaseAddress"]}/api/PersonTracking/{buildingId}/{tranDate}";
            PersonAccess person = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode && !response.ReasonPhrase.Equals("No Content"))
            {
                string json = await response.Content.ReadAsStringAsync();
                person = JsonConvert.DeserializeObject<PersonAccess>(json);
            }

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
