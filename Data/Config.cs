namespace GoogleCRBot.Data
{
    public class Config
    {
        public string ClassroomLink { get; set; }
        public UserConfig User { get; set; }
        public DriverConfig Driver { get; set; }
        public Config(string classroomLink, UserConfig user)
        {
            ClassroomLink = classroomLink;
            User = user;
            Driver = new DriverConfig();
        }
    }
    public class DriverConfig
    {
        public string DriverFolder { get; set; } = "drivers";
        public string PreferredBrowser { get; set; } = "firefox";
        public bool RunHeadless { get; set; } = false;
    }
    public class UserConfig
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserConfig(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public void Deconstruct(out string username, out string password)
        {
            username = Username;
            password = Password;
        }
    }
}