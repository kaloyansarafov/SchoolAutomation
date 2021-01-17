namespace GoogleCRBot.Data
{
    public class Config
    {
        public string ClassroomLink { get; set; }
        public string DriverFolder { get; set; }
        public UserConfig User { get; set; }
        public string PreferredBrowser { get; set; }
    }
    public class UserConfig
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public void Deconstruct(out string username, out string password)
        {
            username = Username;
            password = Password;
        }
    }
}