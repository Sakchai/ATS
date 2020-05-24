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

        public IEnumerable<string> GetFiles(string path, string searchPatterns, SearchOption searchOptions = SearchOption.TopDirectoryOnly)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            var dirs = (from file in dir.EnumerateFiles(searchPatterns, searchOptions)
                        orderby file.CreationTime ascending
                        select file.FullName).Distinct(); // Don't need <string> here, since it's implied
            return dirs;
        }

        public async Task CreateV2Async()
        {
            try
            {
                string sourceDirectory = ConfigurationManager.AppSettings["currentPath"];
                string[] invaild = ConfigurationManager.AppSettings["InvaildText"].Split(',');

                string archiveDirectory = ConfigurationManager.AppSettings["archivePath"];
                int buildingId = Int32.Parse(ConfigurationManager.AppSettings["buildingId"]);

                string searchPatterns = $"*.{ConfigurationManager.AppSettings["ImageType"]}";
                var txtFiles = GetFiles(sourceDirectory, searchPatterns);
                foreach (string currentFile in txtFiles)
                {
                    string ocrString = StartOCR(currentFile);
                    for (int i = 0; i < invaild.Length; i++)
                    {
                        ocrString = ocrString.Replace(invaild[i], string.Empty);
                    }
                    ocrString = ocrString.Replace(Environment.NewLine, " ");
                    ocrString = ocrString.Replace("  ", ",");
                    ocrString = ocrString.Trim();
                    ocrString = ocrString.Replace(" ", ",");
                    DateTime tranDate = File.GetCreationTime(currentFile);
                    tranDate = tranDate.AddTicks(-(tranDate.Ticks % TimeSpan.TicksPerSecond));
                    log.Info($"FileName:{Path.GetFileName(currentFile)},Tran Date:{tranDate}, OCR:{ocrString}");

                    if (!string.IsNullOrWhiteSpace(ocrString))  
                        await CreateOrUpdatePersonAccessAPIV2(buildingId, ocrString, tranDate);

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

        private async Task CreateOrUpdatePersonAccessAPIV2(int buildingId, string ocrString, DateTime creationDate)
        {
            var ocrs = ocrString.Split(",", StringSplitOptions.RemoveEmptyEntries);
            int total = 0;
            int failed = 0;

            switch (ocrs.Length)
            {
                case 1:
                    total = Int32.Parse(ocrs[0]);
                    break;
                case 2:
                    total = Int32.Parse(ocrs[0]);
                    failed = Int32.Parse(ocrs[1]);
                    break;
                default:
                    log.Error($"Invalid OCR string:{ocrString}.");
                    return;
            }

            string tranDate = creationDate.ToString("yyyyMMddHHmmss");
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
                    TranDate = creationDate
                };
                await CreatePersonAccessAsync(personAccessDto);
            }

        }

        private string StartOCR(string Filename)
        {
            using (Stream sr = File.Open(Filename, FileMode.Open))
            {
                try
                {
                    OCRSpace ocr = new OCRSpace();
                    OCRSpaceResponse response = ocr.DoOCR(sr, Filename);

                    if (response != null && !response.IsErroredOnProcessing && response.ParsedResults.Count > 0)
                    {
                        return response.ParsedResults[0].ParsedText;
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                }
            }
            return string.Empty;
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
