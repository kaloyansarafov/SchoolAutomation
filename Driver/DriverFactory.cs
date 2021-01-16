using System;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace GoogleCRBot
{
    internal static class DriverFactory
    {
        const string defaultURI = "http://127.0.0.1";
        internal static IWebDriver InitDriver(string driverFolder, string browser)
        {
            DriverWithExe driver = new DriverWithExe();
            DriverOptions options = getBrowserOptions(browser);
            string driverName = getDriverName(browser);

            string uri = defaultURI + ":" + getDefaultPort(browser);

            try
            {
                Console.WriteLine("Attaching webdriver");
                driver.Connect(new Uri(uri), options);
            }
            catch (OpenQA.Selenium.WebDriverException)
            {
                Console.WriteLine("Starting webdriver");

                string driverFile = $"{driverFolder}/{driverName}";
                if (System.OperatingSystem.IsWindows())
                {
                    driverFile += ".exe";
                }
                driver.StartDriverExe(driverFile);

                // Try to reatach
                driver.Connect(new Uri(uri), options);
            }
            Console.WriteLine("Attached webdriver");
            return driver;
        }

        internal static Data.BrowserConfig GetBrowserData(Data.Config global)
        {
            switch (global.PreferredBrowser)
            {
                case "firefox":
                    return global.Firefox;
                case "chrome":
                    return global.Chrome;
                default:
                    throw new NotImplementedException("Not implemented for browser: " + global.PreferredBrowser);
            }
        }

        private static DriverOptions getBrowserOptions(string browser)
        {
            switch (browser)
            {
                case "firefox":
                    return new OpenQA.Selenium.Firefox.FirefoxOptions();
                case "chrome":
                    return new OpenQA.Selenium.Chrome.ChromeOptions();
                default:
                    throw new NotImplementedException("Not implemented for browser: " + browser);
            }
        }

        private static string getDriverName(string browser)
        {
            switch (browser)
            {
                case "chrome":
                    return "chromedriver";
                case "firefox":
                    return "geckodriver";
                default:
                    throw new NotImplementedException("Not implemented for browser: " + browser);
            }
        }

        private static string getDefaultPort(string browser)
        {
            switch (browser)
            {
                case "chrome":
                    return "9515";
                case "firefox":
                    return "4444";
                default:
                    throw new NotImplementedException("Not implemented for browser: " + browser);
            }
        }
    }
}