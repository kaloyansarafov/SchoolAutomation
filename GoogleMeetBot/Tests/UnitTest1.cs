using NUnit.Framework;
using GoogleMeetBot;
using Newtonsoft.Json;
using System.IO;

namespace Tests
{
    public class Tests
    {
        MeetBot bot;
        [SetUp]
        public void Setup()
        {
            MeetConfig config = JsonConvert.DeserializeObject<MeetConfig>(File.ReadAllText("/home/lyubenk/source/repos/GoogleCRBot/GoogleMeetBot/Tests/config.json"));
            bot = new MeetBot(config);
            bot.Login();
        }

        [Test]
        public void EnterMeet()
        {
            bot.EnterMeet("https://meet.google.com/cbc-hsxd-erc?authuser=1");
        }
        [OneTimeTearDown]
        public void ClassTeardown()
        {
            bot.Dispose();
        }
    }
}