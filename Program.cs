using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using Newtonsoft.Json;
using OpenQA.Selenium.Remote;

namespace GoogleCRBot
{
    class Program
    {
        const string url = "https://classroom.google.com/c/MjEwMzIwNDY5MjYz";
        const string remoteUri = "http://127.0.0.1:4444";
        static void Main()
        {
            using ClassroomBot bot = new ClassroomBot();

            if (bot.Login())
            {
                Console.WriteLine("Sending message");
                bot.SendOnFirst("Добър ден", url);
            }
            Console.ReadLine();
        }
    }
}