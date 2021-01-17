using System;
using System.IO;
using System.Reflection;
using GoogleCRBot;
using GoogleCRBot.Data;
using Newtonsoft.Json;

namespace Main
{
    class Tests
    {
        ClassroomBot bot { get; }
        public Tests(ClassroomBot bot)
        {
            this.bot = bot;
        }
        public void WriteOnMessage()
        {
            Console.WriteLine("Writing message");
            bot.WriteOnMessage("Добър ден", 1);
        }
        public void WriteOnPost()
        {
            bot.GoToPost(0);
            bot.WriteOnCurrentPost("Добре");
            Console.WriteLine("Press enter to go back...");
            Console.ReadLine();
            bot.GoHome();
        }
        public void FetchPostOverview()
        {
            Post item = bot.GetPostOverview(0);
            Console.WriteLine(item.Teacher);
            Console.WriteLine(item.Timestamp);
            Console.WriteLine(item.Name);
        }
        public void FetchMessage()
        {
            Message post = bot.GetMessage(0);
            Console.WriteLine(post.Teacher);
            Console.WriteLine(post.Timestamp);
            Console.WriteLine(post.Information);
        }
    }
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            using var bot = new ClassroomBot(config);
            bot.Login();
            Console.WriteLine(bot.GetMessage(0));
            Console.WriteLine(bot.GetPostOverview(0));
            Console.WriteLine(bot.GoToPost(0));
            bot.GoHome();
            Console.WriteLine(bot.GetMessage(0));
            // Test(new Tests(bot));
        }
        static void Test(Tests tests)
        {
            var methods = tests.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                BindingFlags.Public);
            foreach (MethodInfo method in methods)
            {
                Console.WriteLine($"{method.Name}: ");
                method.Invoke(tests, null);
                Console.WriteLine("\n");
                Console.WriteLine("Done. Press enter to continue...");
                Console.ReadLine();
            }
            Console.WriteLine("Finished.");
        }
    }
}