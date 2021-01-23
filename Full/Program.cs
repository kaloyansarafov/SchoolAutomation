using System;
using GBot;
using GCRBot;
using MeetGBot;

namespace Full
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!System.IO.File.Exists("config.json"))
            {
                ClassroomBot.CreateEmpty<Config>();
                Console.WriteLine("Created sample config.json");
                return;
            }
        }
    }
}
