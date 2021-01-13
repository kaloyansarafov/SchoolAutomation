using System.Collections.Generic;

namespace GoogleCRBot
{
    public class Data
    {
        public string ClassroomLink { get; set; }
        public string RemoteURI { get; set; }
        public string DriverFolder { get; set; }
        public string LogFolder { get; set; }
        public Dictionary<string, Dictionary<string, int>> Delays;
    }
}