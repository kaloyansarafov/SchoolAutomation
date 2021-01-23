using System;

namespace GBot
{
    public static class Cookies
    {
        private const string CookiesFile = "cookies.json";
        public static string GetName(string browser)
        {
            switch (browser)
            {
                case "chrome":
                    return "chrome-" + CookiesFile;
                case "firefox":
                    return "firefox-" + CookiesFile;
                default:
                    throw new NotSupportedException(browser);
            }
        }
    }
}