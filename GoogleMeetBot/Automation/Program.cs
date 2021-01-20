using System;
using System.IO;
using System.Threading.Tasks;
using GoogleMeetBot;
using Newtonsoft.Json;
using OpenQA.Selenium.Firefox;

namespace Automation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            FirefoxProfile profile = new();
            FirefoxOptions opts = new();
            opts.Profile = profile;
            string driverPath = Path.Combine(Path.GetFullPath("."), "drivers");
            using (var fd = new FirefoxDriver(driverPath, opts))
            {
                Console.WriteLine("Enter");
                Console.ReadLine();
            }
            // await MeetTest();
        }

        private static async Task MeetTest()
        {
            MeetConfig config = JsonConvert.DeserializeObject<MeetConfig>(File.ReadAllText(Path.GetFullPath(".") + "/config.json"));
            MeetBot bot = new MeetBot(config);
            bool loggedIn = bot.Login();
            if (!loggedIn)
            {
                Console.WriteLine("Retrying login");
                for (int i = 0; i < 3 && !loggedIn; i++)
                {
                    loggedIn = bot.Login();
                }
                Console.WriteLine("Logged in: " + loggedIn);
            }
            bot.EnterMeet("https://meet.google.com/rje-zpyi-jcg");
            await Task.Delay(5000);
            bot.LeaveMeet();
        }
    }
}
