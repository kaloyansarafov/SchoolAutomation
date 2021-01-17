using System;
using GoogleCRBot.Data;
using OpenQA.Selenium;

namespace GoogleCRBot
{
    internal static class DriverFactory
    {
        const string defaultURI = "http://127.0.0.1";
        internal static IWebDriver InitDriver(DriverConfig config)
        {
            DriverWithExe driver = new DriverWithExe();
            DriverOptions options = getBrowserOptions(config.PreferredBrowser, config.RunHeadless);
            string driverName = getDriverName(config.PreferredBrowser);

            string uri = defaultURI + ":" + getDefaultPort(config.PreferredBrowser);

            try
            {
                Console.WriteLine("Attaching webdriver");
                driver.Connect(new Uri(uri), options);
            }
            catch (OpenQA.Selenium.WebDriverException)
            {
                Console.WriteLine("Starting webdriver");

                string driverFile = $"{config.DriverFolder}/{driverName}";
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

        private static DriverOptions getBrowserOptions(string browser, bool headless)
        {
            switch (browser)
            {
                case "firefox":
                    var ffOpts = new OpenQA.Selenium.Firefox.FirefoxOptions();

                    if (headless) ffOpts.AddArgument("--headless");

                    return ffOpts;
                case "chrome":
                    var chromeOpts = new OpenQA.Selenium.Chrome.ChromeOptions();

                    if (headless) chromeOpts.AddArgument("--headless");

                    return chromeOpts;
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