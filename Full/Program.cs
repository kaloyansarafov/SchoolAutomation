using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GBot;
using GCRBot;
using GCRBot.Data;
using MeetGBot;
using Newtonsoft.Json;
using NLog;

namespace Full
{
    class Program
    {
        const string meetLinkPattern = @"https:\/\/meet.google.com\/([A-Za-z]{3}-?)([a-zA-Z]{4}-?)([A-Za-z]{3}-?)";
        const string timestampPattern = @"([1-9]{2}:[0-9]{2})";

        static NLog.Logger logger;
        static MeetBot meetBot;

        static string DefaultMeetLink;
        static bool NemskaGrupa;

        static TimeSpan threeMins = new TimeSpan(0, 3, 0);
        static TimeSpan twoMins = new TimeSpan(0, 2, 0);
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (!System.IO.File.Exists("config.json"))
            {
                ClassroomBot.CreateEmpty<LangConfig>();
                logger.Info("Created sample config.json");
                return;
            }
            SetupLogger();
            logger = LogManager.GetCurrentClassLogger();

            LangConfig config = JsonConvert.DeserializeObject<LangConfig>(File.ReadAllText("config.json"));
            if (config == null) logger.Error("Config is null");
            else logger.Trace("Loaded config");

            NemskaGrupa = config.NemskaGrupa;
            DefaultMeetLink = config.DefaultMeetLink;
            ClassroomBotter botter = new ClassroomBotter(config);
            meetBot = new MeetBot(config);
            botter.OnMessage += MessageReceivedAsync;
            botter.OnPost += PostReceived;
            try
            {
                Task crStart = botter.Start();
                Console.ReadLine();
                botter.Stop();
                await crStart;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Main loop caught an exception");
            }
            finally
            {
                meetBot.Dispose();
            }
        }

        private static void SetupLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            logconsole.Layout = new NLog.Layouts.SimpleLayout(
                "[${date:format=HH\\:mm\\:ss}][${level}]${logger:shortName=true}: ${message}"
            );

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole, "*", final: true);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }

        private static void PostReceived(object bot, DataEventArgs<Post> e)
        {
            logger.Info("Received post: " + e.Data.Name);
        }

        static Message lastMessage = null;
        static object msgLock = new object();
        static Task MeetSession = null;
        private static void MessageReceivedAsync(object sender, DataEventArgs<Message> e)
        {
            logger.Info("Got message from " + e.Data.Teacher);
            logger.Info(e.Data);
            Message latestMsg = e.Data;
            if (latestMsg.Information.ContainsGreeting()
                || latestMsg.Information.HasMeetLink())
            {
                if (!AreLangClass(latestMsg, lastMessage))
                {
                    ClassroomBot bot = sender as ClassroomBot;
                    if (bot == null) throw new Exception("Classroom bot is null");
                    latestMsg = LangGroupFilter(bot, latestMsg);
                    logger.Debug(latestMsg);
                    bot.SendOnMessage(latestMsg, "Добър ден.");
                    logger.Info("Добър ден " + latestMsg.Teacher);
                    logger.Info("Няма здрасти за " + latestMsg.Teacher);
                    if (latestMsg.Information.HasMeetLink())
                    {
                        Regex linkReg = new Regex(meetLinkPattern);
                        string link = linkReg.Match(latestMsg.Information).Value;
                        Regex timeReg = new Regex(timestampPattern);
                        string time = timeReg.Match(latestMsg.Information).Value;
                        var dt = DateTime.Parse(time);
                        MeetSession = JoinMeet(link, dt);
                    }
                    else
                    {
                        MeetSession = JoinMeet(DefaultMeetLink);
                    }
                    lock (msgLock)
                        lastMessage = latestMsg;
                }
            }
        }
        private static async Task LeaveMeet()
        {
            while (meetBot.State == MeetState.InCall)
            {
                int peopleInMeet = meetBot.PeopleInMeet();
                bool inLangClass = false;
                lock (msgLock)
                {
                    string teacher = lastMessage.Teacher;
                    inLangClass = teacher.Contains("Вихрогонова") || teacher.Contains("Чапанова");
                }
                if (inLangClass && peopleInMeet < 6)
                {
                    meetBot.LeaveMeet();
                }
                else if (!inLangClass && peopleInMeet < 14)
                {
                    meetBot.LeaveMeet();
                }
                await Task.Delay(threeMins);
            }
        }
        private static async Task JoinMeet(string link, DateTime time = default)
        {
            if (string.IsNullOrEmpty(link))
            {
                throw new Exception("Invalid meet link");
            }
            if (meetBot.State == MeetState.NotLoggedIn) meetBot.Login();
            if (meetBot.State != MeetState.OutsideMeet)
                throw new Exception("Meetbot with invalid state: " + meetBot.State);

            if (time != default)
            {
                logger.Debug("Waiting until " + time + " : " + (time - DateTime.Now));
                await Task.Delay(time - DateTime.Now);
            }
            else
            {
                logger.Debug("Waiting " + threeMins);
                await Task.Delay(threeMins);
            }
            logger.Info("Joining: " + link);
            try
            {
                meetBot.EnterMeetOverview(link);
            }
            catch (OpenQA.Selenium.WebDriverTimeoutException)
            {
                if (link.Contains("/lookup/"))
                {
                    logger.Error("No meet in lookup link");
                }
                else throw;
            }
            if (meetBot.State != MeetState.InOverview)
            {
                throw new Exception("Not in overview");
            }

            //TODO Replace with teacher in call check
            int people = meetBot.PeopleInMeetOverview();
            logger.Info("People in meet: " + people);
            if (people > 15)
            {
                logger.Info("Entering meet...");
                meetBot.EnterMeet();
                await LeaveMeet();
            }
            else
            {
                logger.Warn("Not enough people to enter");
                logger.Info("Trying again...");
                for (int i = 0; i < 3; i++)
                {
                    people = meetBot.PeopleInMeetOverview();
                    logger.Debug("People in meet: " + people);
                    if (people > 15)
                    {
                        break;
                    }
                    await Task.Delay(twoMins);
                }
                logger.Info("Entering meet...");
                meetBot.EnterMeet();
                await LeaveMeet();
            }
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
                    logger.Trace("Nemska grupa found teacher");
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
                if (msgAfter.Teacher.Contains("Чапанова"))
                {
                    logger.Trace("Ruska grupa found teacher");
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
