using System;
using System.IO;
using System.Threading.Tasks;
using GoogleMeetBot;
using Newtonsoft.Json;

namespace Automation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MeetConfig config = JsonConvert.DeserializeObject<MeetConfig>(File.ReadAllText(Path.GetFullPath(".") + "/config.json"));
            using MeetBot bot = new MeetBot(config);
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
            Console.ReadLine();
            bot.EnterMeet("https://meet.google.com/epu-mgad-opq?authuser=1");
            await Task.Delay(5000);
            bot.LeaveMeet();
        }
    }
}
