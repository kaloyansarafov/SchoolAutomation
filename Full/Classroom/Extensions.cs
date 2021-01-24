using System;

namespace Full
{
    internal static class Extensions
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
            return text.Contains("meet.google.com");
        }
    }
}