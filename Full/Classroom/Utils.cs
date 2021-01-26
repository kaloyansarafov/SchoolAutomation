using System;
using System.Text.RegularExpressions;
using GCRBot.Data;

namespace Full
{
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
        public static bool HasMeetLink(this string text)
        {
            return text.Contains("https://meet.google.com");
        }
    }
    public static class Utils
    {
        public static bool Retry(Func<bool> predicate, int times = -1)
        {
            bool success = predicate();
            if (!success)
            {
                if (times > 0)
                {
                    for (int i = 0; i < times && !success; i++)
                    {
                        success = predicate();
                    }
                }
                else
                {
                    while (!success)
                    {
                        success = predicate();
                    }
                }
            }
            return success;
        }
        public static bool IsLangClass(Message msg)
        {
            if (msg == null)
            {
                return false;
            }
            return msg.Teacher.Contains("Чапанова") || msg.Teacher.Contains("Вихрогонова");
        }
        public static string GetMeetLink(string text)
        {
            Regex meetLinkReg = new Regex(@"https:\/\/meet.google.com\/([A-Za-z]{3}-?)([a-zA-Z]{4}-?)([A-Za-z]{3}-?)");
            Match match = meetLinkReg.Match(text);
            return match.Value;
        }
    }
}