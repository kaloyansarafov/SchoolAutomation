namespace GBot
{
    public class Config
    {
        public string Link { get; set; }
        public DriverConfig Driver { get; set; } = new DriverConfig();
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
        public string DriverFolder { get; set; } = "drivers";
        public string PreferredBrowser { get; set; } = "firefox";
        public bool RunHeadless { get; set; } = false;
    }
}