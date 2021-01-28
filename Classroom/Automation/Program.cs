using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GBot;
using GCRBot;
using GCRBot.Data;
using Newtonsoft.Json;

namespace Automation
{
    class Program
    {
        static NLog.Logger logger;
        static void Main(string[] args)
        {
            SetupLogger();
            Config config = GetConfig();
            if (config == null) return;
            // config.Driver.Browser = "firefox";
            config.Driver.Headless = false;
            using (ClassroomBot bot = new ClassroomBot(config))
            {
                try
                {
                    logger.Info("Loggedin: " + bot.Login());
                    logger.Info(bot.GetClassroomMeetLink());
                    Post post = bot.GetPost(0);
                    logger.Info(post);
                    logger.Info(bot.GetPostAfter(post));
                    Message msg = bot.GetMessage(0);
                    logger.Info(msg);
                    logger.Info("Written comment? {0}", bot.WrittenCommentOn(msg));
                    // logger.Info(bot.GetMessageAfter(msg).Teacher);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Caught: " + ex);
                }
                Console.ReadLine();
            }
        }
        static void SetupLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            logconsole.Layout = new NLog.Layouts.SimpleLayout(
                "[${date:format=HH\\:mm\\:ss}][${level}]${logger:shortName=true}: ${message}"
            );
            logconsole.Encoding = System.Text.Encoding.UTF8;

            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logconsole, "*", final: true);

            // Apply config           
            NLog.LogManager.Configuration = config;
            logger = NLog.LogManager.GetCurrentClassLogger();
        }
        static Config GetConfig()
        {
            if (!File.Exists("config.json"))
            {
                ClassroomBot.CreateEmpty<Config>();
                Console.WriteLine("Created sample config.json");
                return null;
            }
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
        }
        private static T LoadFromJson<T>(string filename)
        {
            if (!File.Exists(filename))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
        }
        private static void SaveToJson<T>(T obj, string saveAs)
        {
            if (obj == null || obj.Equals(default(T)))
            {
                Console.WriteLine("Null or default object: " + saveAs);
                return;
            }
            string json = JsonConvert.SerializeObject(obj);
            Console.WriteLine(json);
            using (StreamWriter sw = new StreamWriter(saveAs))
            {
                sw.WriteLine(json);
            }
        }
    }
    public static class Extensions
    {
        public static bool ContainsGreeting(this string text)
        {
            string[] greetings = new string[] {
                "добър ден","привет","здравейте","hello","очаквам ви","guten tag","good morning","добро утро"
            };
            foreach (string greeting in greetings)
            {
                if (text.Contains(greeting, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsMeetLink(this string text)
        {
            return text.Contains("meet.google.com");
        }
    }
}