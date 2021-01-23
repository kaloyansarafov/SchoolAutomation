namespace GCRBot.Data
{
    public class CRConfig : GBot.Config
    {
        public User User { get; }

        public CRConfig(User user, string link) : base(link)
        {
            User = user;
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
        public void Deconstruct(out string username, out string password)
        {
            (username, password) = (Username, Password);
        }
    }
}