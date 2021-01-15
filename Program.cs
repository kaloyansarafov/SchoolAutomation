using System;
using GoogleCRBot;

namespace Main
{
    class Program
    {
        static void Main()
        {
            using ClassroomBot bot = new ClassroomBot();

            if (bot.Login())
            {
                Console.WriteLine("Writing message");
                bot.WriteOnFirst("Добър ден");
            }
            Console.ReadLine();
        }
    }
}