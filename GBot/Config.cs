namespace GBot
{
    public record Config
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
    public record DriverConfig
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