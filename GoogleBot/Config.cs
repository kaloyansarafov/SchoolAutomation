namespace GoogleBot
{
    public class Config
    {
        public string Link { get; set; }
        public DriverConfig Driver { get; set; }
        public Config(string link)
        {
            Link = link;
            Driver = new DriverConfig();
        }
    }
    public class DriverConfig
    {
        public string DriverFolder { get; set; } = "drivers";
        public string PreferredBrowser { get; set; } = "firefox";
        public bool RunHeadless { get; set; } = false;
    }
}