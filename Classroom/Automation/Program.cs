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
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            CancellationTokenSource source = new();
            var config = GetConfig();
            if (config == null) return;
            var bot = new ClassroomBot(config);
            try
            {
                Task t = Task.Run(() =>
                {
                    try
                    {
                        Console.WriteLine("LoggedIn: " + bot.Login());
                        bot.GoHome();
                        Console.WriteLine(bot.GetMessage(0));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Caught: " + ex.Message);
                    }
                }, source.Token);
                Console.WriteLine("Press enter to stop.");
                Console.ReadLine();
                source.Cancel();
                t.Wait();
            }
            finally
            {
                bot.Dispose();
            }
        }
        static void AutomationScript()
        {
            var config = GetConfig();
            if (config == null) return;

            CancellationTokenSource source = new();
            Bot bot = new Bot(config, HelloMsg, source.Token);
            try
            {

                DateTime start = DateTime.Now;
                Task endTime = Task.Run(() =>
                {
                    while (true)
                    {
                        TimeSpan elapsed = DateTime.Now - start;
                        if (elapsed.Hours == 5)
                        {
                            source.Cancel();
                            break;
                        }
                    }
                }, source.Token);
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
                source.Cancel();
            }
            finally
            {
                bot.Dispose();
            }

        }
        const bool NemskaGrupa = false;
        static Task HelloMsg(ClassroomBot bot, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                Message lastMessage = LoadFromJson<Message>("lastMessage.json");
                try
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            SaveToJson(lastMessage, "lastMessage.json");
                            Console.WriteLine("Saved last message");
                            break;
                        }
                        Message latestMsg = bot.GetMessage(0);
                        if (latestMsg != lastMessage)
                        {
                            Console.WriteLine(latestMsg);
                            if (latestMsg.Information.ContainsGreeting()
                                || latestMsg.Information.IsMeetLink())
                            {
                                if (!AreLangClass(latestMsg, lastMessage))
                                {
                                    latestMsg = LangGroupFilter(bot, latestMsg);
                                    bot.SendOnMessage(latestMsg, "Добър ден.");
                                    Console.WriteLine("Добър ден " + latestMsg.Teacher);
                                }
                            }
                        }
                        lastMessage = latestMsg;

                        await Task.Delay(new TimeSpan(0, minutes: 1, 0), token);
                    }
                }
                catch (TaskCanceledException)
                {
                    SaveToJson(lastMessage, "lastMessage.json");
                    Console.WriteLine("Saved last message");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{nameof(HelloMsg)} caught: " + ex.Message);
                }
                Console.WriteLine($"{nameof(HelloMsg)} is done.");
            });
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
    public class Bot : IDisposable
    {
        protected ClassroomBot bot;
        private readonly CancellationToken token;

        public Task Task { get; }
        public Bot(Config config, Func<ClassroomBot, CancellationToken, Task> loop, CancellationToken token)
        {
            bot = new ClassroomBot(config);
            Login();
            this.token = token;
            Task = loop(bot, token);
        }
        void Login()
        {
            bool loggedIn = bot.Login();
            Console.WriteLine("Logged in: " + loggedIn);
            if (!loggedIn)
            {
                for (int i = 0; i < 3 && !loggedIn; i++)
                {
                    Console.WriteLine("Retrying login");
                    loggedIn = bot.Login();
                }
                if (!loggedIn) throw new Exception("Can't login");
                Console.WriteLine("Logged in");
            }
        }

        public void Dispose()
        {
            ((IDisposable)bot).Dispose();
            Task.Dispose();
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