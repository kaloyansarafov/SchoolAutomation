using System;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace GoogleCRBot
{
    public static class BrowserFactory
    {
        public static StreamWriter Logger { get; set; }
        static string defaultURI = "http://127.0.0.1";
        public static Data.BrowserData GetBrowserData(Data.GlobalData global)
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
        public static IWebDriver InitDriver(string driverFolder, string browser)
        {
            IWebDriver driver;
            DriverOptions options = getBrowserOptions(browser);
            string driverName = getDriverName(browser);

            defaultURI = defaultURI + ":" + getDefaultPort(browser);

            try
            {
                Console.WriteLine("Attaching webdriver");
                driver = new RemoteWebDriver(new Uri(defaultURI), options);
            }
            catch (OpenQA.Selenium.WebDriverException)
            {
                Console.WriteLine("Starting webdriver");
                startWebDriver($"{driverFolder}/{driverName}.exe");

                // Try to reatach
                driver = new RemoteWebDriver(new Uri(defaultURI), options);
            }
            // catch (InvalidOperationException)
            // {
            // Could restart web driver
            // }
            Console.WriteLine("Attached webdriver");
            return driver;
        }

        private static void startWebDriver(string driverPath)
        {
            if (!File.Exists(driverPath))
            {
                throw new FileNotFoundException($"No such file: {driverPath}");
            }
            Process proc = new Process();
            proc.StartInfo.FileName = driverPath;
            proc.StartInfo.RedirectStandardError = true;

            if (!proc.Start())
            {
                throw new OperationCanceledException("Couldn't start webdriver");
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