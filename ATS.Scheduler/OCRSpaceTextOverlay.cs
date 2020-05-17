using System.Collections.Generic;

namespace ATS.Scheduler
{
    public class OCRSpaceTextOverlay
    {
        public List<OCRSpaceLine> Lines { get; set; }
        public bool HasOverlay { get; set; }
        public string Message { get; set; }
    }
}
