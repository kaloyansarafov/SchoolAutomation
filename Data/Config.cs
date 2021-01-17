namespace GoogleCRBot.Data
{
    public class Config
    {
        public string ClassroomLink { get; }
        public UserConfig User { get; }
        public string DriverFolder { get; set; } = "driver";
        public string PreferredBrowser { get; set; } = "firefox";
        public Config(string classroomLink, UserConfig user)
        {
            ClassroomLink = classroomLink;
            User = user;
        }
    }
    public record UserConfig
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