namespace GBot
{
    public class Config
    {
        public string Link { get; }
        public DriverConfig Driver { get; } = new DriverConfig();
        public string CookieFolder { get; set; } = "./";
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
        public string DriverFolder { get; set; }
        public string Browser { get; set; }
        public bool Headless { get; set; }
        public DriverConfig(string driverFolder = "drivers", string browser = "firefox", bool headless = false)
        {
            DriverFolder = driverFolder;
            Browser = browser;
            Headless = headless;
        }
    }
}