using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GBot;
using GCRBot;
using GCRBot.Data;
using MeetGBot;
using Newtonsoft.Json;

namespace Full
{
    class Program
    {
        const string meetLinkPattern = @"https:\/\/meet.google.com\/([A-Za-z]{3}-?)([a-zA-Z]{4}-?)([A-Za-z]{3}-?)";
        const string timestampPattern = @"([1-9]{2}:[0-9]{2})";
        static MeetBot meetBot;
        private static string DefaultMeetLink;
        static bool NemskaGrupa;
        static TimeSpan toWait = new TimeSpan(0, 5, 0);
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (!System.IO.File.Exists("config.json"))
            {
                ClassroomBot.CreateEmpty<LangConfig>();
                Console.WriteLine("Created sample config.json");
                return;
            }
            LangConfig config = JsonConvert.DeserializeObject<LangConfig>(File.ReadAllText("config.json"));
            NemskaGrupa = config.NemskaGrupa;
            DefaultMeetLink = config.DefaultMeetLink;
            ClassroomBotter botter = new ClassroomBotter(config);
            meetBot = new MeetBot(config);
            botter.OnMessage += MessageReceived;
            try
            {
                Task crStart = botter.Start();
                Console.ReadLine();
                botter.Stop();
                await crStart;
            }
            finally
            {
                meetBot.Dispose();
            }
        }
        static Message lastMessage = null;
        private static void MessageReceived(object bot, MessageEventArgs e)
        {
            Console.WriteLine("Got message from " + e.Message.Teacher);
            Console.WriteLine(e.Message);
            // Console.WriteLine(latestMsg);
            Message latestMsg = e.Message;
            if (latestMsg.Information.ContainsGreeting()
                || latestMsg.Information.HasMeetLink())
            {
                if (!AreLangClass(latestMsg, lastMessage))
                {
                    latestMsg = LangGroupFilter(bot as ClassroomBot, latestMsg);
                    // bot.SendOnMessage(latestMsg, "Добър ден.");
                    Console.WriteLine("Добър ден " + latestMsg.Teacher);
                    // latestMsg = new Message { Information = "Очаквам ви в 18:10ч.  https://meet.google.com/rje-zpyi-jcg" };
                    if (latestMsg.Information.HasMeetLink())
                    {
                        Regex linkReg = new Regex(meetLinkPattern);
                        string link = linkReg.Match(latestMsg.Information).Value;
                        Regex timeReg = new Regex(timestampPattern);
                        string time = timeReg.Match(latestMsg.Information).Value;
                        var dt = DateTime.Parse(time);
                        JoinMeet(link, dt);
                    }
                    else
                    {
                        JoinMeet(DefaultMeetLink);
                    }
                }
            }
        }
        private static void JoinMeet(string link, DateTime time = default)
        {
            if (string.IsNullOrEmpty(link))
            {
                throw new Exception("Invalid meet link");
            }
            if (meetBot.State == MeetState.NotLoggedIn) meetBot.Login();
            if (time != default)
            {
                Console.WriteLine("Waiting until " + time + " : " + (time - DateTime.Now));
            }
            else
            {
                Console.WriteLine("Waiting " + toWait);
            }
            Console.WriteLine("Joining: " + link);
            try
            {
                meetBot.EnterMeetOverview(link);
            }
            catch (OpenQA.Selenium.WebDriverTimeoutException)
            {
                if (link.Contains("/lookup/"))
                {
                    Console.WriteLine("No meet in lookup link");
                }
                else throw;
            }
            if (meetBot.State == MeetState.InOverview)
                Console.WriteLine("People in meet: " + meetBot.PeopleInMeetOverview());
        }
        private static bool AreLangClass(Message latestMsg, Message lastMessage)
        {
            if (lastMessage == null)
            {
                return false;
            }
            if (latestMsg.Teacher.Contains("Чапанова"))
            {
                return lastMessage.Teacher.Contains("Вихрогонова");
            }
            else if (latestMsg.Teacher.Contains("Вихрогонова"))
            {
                return lastMessage.Teacher.Contains("Чапанова");
            }
            return false;
        }

        private static Message LangGroupFilter(ClassroomBot bot, Message latest)
        {
            if (NemskaGrupa && latest.Teacher.Contains("Чапанова"))
            {
                Message msgAfter = bot.GetMessageAfter(latest, 0);
                if (msgAfter.Teacher.Contains("Вихрогонова"))
                {
                    latest = msgAfter;
                }
                else
                {
                    latest = null;
                }
            }
            else if (!NemskaGrupa && latest.Teacher.Contains("Вихрогонова"))
            {
                Message msgAfter = bot.GetMessageAfter(latest, 1);
                Console.WriteLine(msgAfter);
                if (msgAfter.Teacher.Contains("Чапанова"))
                {
                    latest = msgAfter;
                }
                else
                {
                    latest = null;
                }
            }

            return latest;
        }
    }
}
