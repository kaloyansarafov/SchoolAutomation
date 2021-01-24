using System;

namespace GBot
{
    public class Config
    {
        public string Link { get; init; }
        public DriverConfig Driver { get; } = new DriverConfig();
        public Config(string link)
        {
            Link = link;
        }
        public Config()
        {
            Link = "";
        }
    }
    public class DriverConfig
    {
        public string Browser { get; set; } = "firefox";
        public string DriverFolder { get; set; }
        public string CookieFolder { get; set; }
        [NonSerialized]
        public bool Headless = true;
        public DriverConfig()
        {
            string currentFolder = "." + System.IO.Path.DirectorySeparatorChar;
            DriverFolder = currentFolder;
            CookieFolder = currentFolder;
        }
    }
}