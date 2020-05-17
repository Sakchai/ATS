using System.Collections.Generic;

namespace ATS.Scheduler
{
    public class OCRSpaceResponse
    {
        public List<OCRSpaceParsedResult> ParsedResults { get; set; }
        public int OCRExitCode { get; set; }
        public bool IsErroredOnProcessing { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }
        public string ProcessingTimeInMilliseconds { get; set; }
    }
}
