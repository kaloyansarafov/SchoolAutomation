using System.Collections.Generic;

namespace GoogleCRBot.Data
{
    public class GlobalData
    {
        public string ClassroomLink { get; set; }
        public string PreferredBrowser { get; set; }
        public string DriverFolder { get; set; }
        public BrowserData Chrome { get; set; }
        public BrowserData Firefox { get; set; }
    }
    public class BrowserData
    {
        public Dictionary<string, Dictionary<string, float>> Delays;
    }
}