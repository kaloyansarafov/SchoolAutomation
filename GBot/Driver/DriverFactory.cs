using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace GBot
{
    public static class DriverFactory
    {
        const string defaultURI = "http://127.0.0.1";
        public static IWebDriver InitDriver(DriverConfig config)
        {
            string driverName = getDriverName(config.Browser);
            DriverOptions options = getBrowserOptions(config);
            IWebDriver driver = getDriver(config, options);
            return driver;
        }

        private static IWebDriver getDriver(DriverConfig config, DriverOptions options)
        {
            IWebDriver driver;
            switch (config.Browser)
            {
                case "firefox":
                    driver = new FirefoxDriver(config.DriverFolder, (FirefoxOptions)options);
                    break;
                case "chrome":
                    driver = new ChromeDriver(config.DriverFolder, (ChromeOptions)options);
                    break;
                default:
                    throw new NotImplementedException("Not implemented for browser: " + config.Browser);
            }

            return driver;
        }

        private static DriverOptions getBrowserOptions(DriverConfig config)
        {
            DriverOptions opts;
            switch (config.Browser)
            {
                case "firefox":
                    var ffOpts = new OpenQA.Selenium.Firefox.FirefoxOptions();

                    if (config.Headless) ffOpts.AddArgument("--headless");
                    ffOpts.LogLevel = FirefoxDriverLogLevel.Fatal;

                    opts = ffOpts;
                    break;
                case "chrome":
                    var chromeOpts = new OpenQA.Selenium.Chrome.ChromeOptions();
                    if (config.Headless) chromeOpts.AddArgument("--headless");
                    chromeOpts.AddArgument("--log-level=3");
                    chromeOpts.AddArgument("--silent");

                    opts = chromeOpts;
                    break;
                default:
                    throw new NotImplementedException("Not implemented for browser: " + config.Browser);
            }
            opts.SetLoggingPreference(LogType.Browser, LogLevel.Off);
            opts.SetLoggingPreference(LogType.Driver, LogLevel.Off);
            opts.SetLoggingPreference(LogType.Client, LogLevel.Off);
            opts.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
            opts.SetLoggingPreference(LogType.Server, LogLevel.Off);
            return opts;
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