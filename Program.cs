using System;
using System.Threading.Tasks;
using GoogleCRBot;
using GoogleCRBot.Data;

namespace Main
{
    class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            using ClassroomBot bot = new ClassroomBot();
            bool loggedIn = await bot.Login();
            if (!loggedIn)
            {
                Console.WriteLine("Failed to login");
                return;
            }
            WriteTest(bot);
            PostTest(bot);
            Console.ReadLine();
        }
        static void WriteTest(ClassroomBot bot)
        {
            Console.WriteLine("Writing message");
            bot.WriteOnPost("Добър ден", 1);
        }
        static void PostTest(ClassroomBot bot)
        {
            Post post = bot.GetPost(1);
            Console.WriteLine(post.Name);
            Console.WriteLine(post.Timestamp);
            Console.WriteLine(post.Message);
        }
    }
}