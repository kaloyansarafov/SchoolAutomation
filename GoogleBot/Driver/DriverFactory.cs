using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace GoogleBot
{
    public static class DriverFactory
    {
        const string defaultURI = "http://127.0.0.1";
        public static IWebDriver InitDriver(DriverConfig config)
        {
            // DriverWithExe driver = new DriverWithExe();
            string driverName = getDriverName(config.PreferredBrowser);
            DriverOptions options = getBrowserOptions(config);
            IWebDriver driver = getDriver(config, options);
            // string uri = defaultURI + ":" + getDefaultPort(config.PreferredBrowser);

            // try
            // {
            //     // Console.WriteLine("Attaching webdriver");
            //     driver.Connect(new Uri(uri), options);
            // }
            // catch (OpenQA.Selenium.WebDriverException)
            // {
            //     // Console.WriteLine("Starting webdriver");

            //     string driverFile = $"{config.DriverFolder}/{driverName}";
            //     if (System.OperatingSystem.IsWindows())
            //     {
            //         driverFile += ".exe";
            //     }
            //     driver.StartDriverExe(driverFile);

            //     // Try to reatach
            //     driver.Connect(new Uri(uri), options);
            // }
            // Console.WriteLine("Attached webdriver");
            return driver;
        }

        private static IWebDriver getDriver(DriverConfig config, DriverOptions options)
        {
            IWebDriver driver;
            switch (config.PreferredBrowser)
            {
                case "firefox":
                    driver = new FirefoxDriver(config.DriverFolder, (FirefoxOptions)options);
                    break;
                case "chrome":
                    driver = new ChromeDriver(config.DriverFolder, (ChromeOptions)options);
                    break;
                default:
                    throw new NotImplementedException("Not implemented for browser: " + config.PreferredBrowser);
            }

            return driver;
        }

        private static DriverOptions getBrowserOptions(DriverConfig config)
        {
            switch (config.PreferredBrowser)
            {
                case "firefox":
                    var ffOpts = new OpenQA.Selenium.Firefox.FirefoxOptions();

                    if (config.RunHeadless) ffOpts.AddArgument("--headless");

                    return ffOpts;
                case "chrome":
                    var chromeOpts = new OpenQA.Selenium.Chrome.ChromeOptions();

                    if (config.RunHeadless) chromeOpts.AddArgument("--headless");

                    return chromeOpts;
                default:
                    throw new NotImplementedException("Not implemented for browser: " + config.PreferredBrowser);
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