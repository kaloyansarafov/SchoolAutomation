namespace GoogleCRBot.Data
{
    public class Config
    {
        public string ClassroomLink { get; set; }
        public string DriverFolder { get; set; }
        public UserConfig User { get; set; }
        public string PreferredBrowser { get; set; }
        public BrowserConfig Chrome { get; set; }
        public BrowserConfig Firefox { get; set; }
    }
}