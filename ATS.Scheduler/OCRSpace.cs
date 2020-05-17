using Newtonsoft.Json;
using ShareX.UploadersLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Text;

namespace ATS.Scheduler
{
    public enum OCRSpaceLanguages
    {
        [Description("Czech")]
        ce,
        [Description("Danish")]
        dan,
        [Description("Dutch")]
        dut,
        [Description("English")]
        eng,
        [Description("Finnish")]
        fin,
        [Description("French")]
        fre,
        [Description("German")]
        ger,
        [Description("Hungarian")]
        hun,
        [Description("Italian")]
        ita,
        [Description("Norwegian")]
        nor,
        [Description("Polish")]
        pol,
        [Description("Portuguese")]
        por,
        [Description("Spanish")]
        spa,
        [Description("Swedish")]
        swe,
        [Description("Chinese Simplified")]
        chs,
        [Description("Greek")]
        gre,
        [Description("Japanese")]
        jpn,
        [Description("Russian")]
        rus,
        [Description("Turkish")]
        tur,
        [Description("Chinese Traditional")]
        cht,
        [Description("Korean")]
        kor
    }

    public class OCRSpace : Uploader
    {
        private const string APIURLFree = "https://api.ocr.space/parse/image";
        private const string APIURLUSA = "?";
        private const string APIURLEurope = "https://apipro3.ocr.space/parse/image"; // Frankfurt
        private const string APIURLAsia = "https://apipro8.ocr.space/parse/image"; // Tokyo

        public OCRSpaceLanguages Language { get; set; } = OCRSpaceLanguages.eng;
        public bool Overlay { get; set; }

        public OCRSpace(OCRSpaceLanguages language = OCRSpaceLanguages.eng, bool overlay = false)
        {
            Language = language;
            Overlay = overlay;
        }

        public OCRSpaceResponse DoOCR(Stream stream, string fileName)
        {
            string APIURL = ConfigurationManager.AppSettings["APIURL"];
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            //arguments.Add("apikey", APIKeys.OCRSpaceAPIKey);
            arguments.Add("apikey", ConfigurationManager.AppSettings["APIKey"]);
            //arguments.Add("url", "");
            arguments.Add("language", Language.ToString());
            arguments.Add("isOverlayRequired", Overlay.ToString());

            UploadResult ur = UploadData(stream, APIURL, fileName, "file", arguments);

            if (ur.IsSuccess)
            {
                return JsonConvert.DeserializeObject<OCRSpaceResponse>(ur.Response);
            }

            return null;
        }
    }
}
