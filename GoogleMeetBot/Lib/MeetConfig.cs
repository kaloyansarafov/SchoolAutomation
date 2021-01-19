namespace GoogleMeetBot
{
    public class MeetConfig : GoogleBot.Config
    {
        public User User { get; }
        public MeetConfig(User user, string link) : base(link)
        {
            this.User = user;
        }
    }

    public class User
    {
        public string Username { get; }
        public string Password { get; }
        public User(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}