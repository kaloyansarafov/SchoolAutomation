using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using GBot;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MeetGBot
{
    public sealed partial class MeetBot : Bot
    {
        readonly WebDriverWait firstLoad;
        readonly WebDriverWait userWait;
        readonly WebDriverWait defaultWait;

        readonly ReadOnlyDictionary<string, By> selectors;

        const string CLASSROOM_URI = "classroom.google.com";
        const string EDU_URI = "edu.google.com";

        static readonly string LoginLink = $"https://{CLASSROOM_URI}/u";
        static readonly string ClassroomLink = $"https://{CLASSROOM_URI}/";
        static readonly string EduLink = $"https://{EDU_URI}";

        readonly string CookiesPath;

        public MeetBot(MeetConfig config) : base(config)
        {
            driver = DriverFactory.InitDriver(config.Driver);
            selectors = SelectorFactory.Get(config.Driver.PreferredBrowser);
            CookiesPath = Cookies.GetPath(config.Driver.PreferredBrowser);

            defaultWait = new WebDriverWait(this.driver, new TimeSpan(0, 0, seconds: 10));
            firstLoad = new WebDriverWait(driver, new TimeSpan(0, 0, seconds: 15));
            userWait = new WebDriverWait(driver, new TimeSpan(0, minutes: 1, 0));
        }


        public void GoHome()
        {
            driver.Navigate().GoToUrl(LoginLink);
        }
        public static void CreateSampleConfig(string directory = "./")
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException("No such dir: " + directory);

            string path = Path.Combine(directory, "config.json");
            if (File.Exists(path))
                throw new Exception("File already exists: " + path);

            var meetConf = new MeetConfig("");
            File.WriteAllText(path, JsonConvert.SerializeObject(meetConf, Formatting.Indented));
        }

        public override bool Login()
        {
            defaultWait.Until(driver => driver.Navigate()).GoToUrl(EduLink);
            firstLoad.Until(driver => driver.Url.Contains(EDU_URI));

            bool loadedCookies = LoadCookies(CookiesPath);

            driver.Navigate().GoToUrl(LoginLink);

            //TODO Replace with factory method
            bool firefox = config.Driver.PreferredBrowser == "firefox";

            try
            {
                bool loggedIn = false;
                if (!loadedCookies)
                {
                    loggedIn = userWait.Until(driver => driver.Url.Contains(ClassroomLink));
                    if (loggedIn)
                    {
                        driver.Navigate().GoToUrl(EduLink);

                        if (firefox) firstLoad.Until(driver => driver.Url.Contains(EDU_URI));

                        SaveCookies(driver.Manage().Cookies.AllCookies, CookiesPath);

                        if (firefox)
                        {
                            driver.Navigate().GoToUrl(LoginLink);
                            loggedIn = firstLoad.Until(driver => driver.Url.Contains(ClassroomLink));
                        }
                    }
                }
                else
                {
                    loggedIn = firstLoad.Until(driver =>
                    {
                        return driver.Url.Contains(CLASSROOM_URI);
                    });
                }
                return loggedIn;

            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine("Timed out: " + ex.Message);
            }
            return false;

        }
    }
}
