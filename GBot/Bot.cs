using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GBot
{
    public abstract class Bot : IDisposable
    {
        protected readonly WebDriverWait firstLoad;
        protected readonly WebDriverWait userWait;
        protected readonly WebDriverWait defaultWait;

        const string CLASSROOM_URI = "classroom.google.com";
        const string EDU_URI = "edu.google.com";

        static readonly string LoginLink = $"https://{CLASSROOM_URI}/u";
        static readonly string ClassroomLink = $"https://{CLASSROOM_URI}";
        static readonly string EduLink = $"https://{EDU_URI}";

        protected readonly string CookiesPath;

        protected Config config { get; }
        protected IWebDriver driver { get; set; }
        public Bot(Config config)
        {
            driver = DriverFactory.InitDriver(config.Driver);
            this.config = config;

            CookiesPath = Path.Combine(config.Driver.CookieFolder, Cookies.GetName(config.Driver.Browser));

            defaultWait = new WebDriverWait(driver, new TimeSpan(0, 0, seconds: 10));
            firstLoad = new WebDriverWait(driver, new TimeSpan(0, 0, seconds: 15));
            userWait = new WebDriverWait(driver, new TimeSpan(0, minutes: 1, 0));
        }

        protected virtual bool Login(bool goToConfigLink = true)
        {
            defaultWait.Until(driver => driver.Navigate()).GoToUrl(EduLink);
            firstLoad.Until(driver => driver.Url.Contains(EDU_URI));

            bool loadedCookies = LoadCookies(CookiesPath);

            driver.Navigate().GoToUrl(LoginLink);

            //TODO Replace with factory method
            bool firefox = config.Driver.Browser == "firefox";

            // if(firefox) checks:
            //   firefox can't insert google classroom cookies into edu.google.com
            try
            {
                bool loggedIn = false;
                if (!loadedCookies)
                {
                    loggedIn = userWait.Until(driver =>
                    {
                        Console.WriteLine($"Matching {driver.Url} against {ClassroomLink}");
                        return driver.Url.Contains(ClassroomLink);
                    });
                    if (loggedIn)
                    {
                        driver.Navigate().GoToUrl(EduLink);

                        // Save edu.google.com cookies insted of classroom's
                        if (firefox) firstLoad.Until(driver => driver.Url.Contains(EDU_URI));

                        SaveCookies(driver.Manage().Cookies.AllCookies, CookiesPath);

                    }
                }
                else
                {
                    loggedIn = firstLoad.Until(driver =>
                    {
                        return driver.Url.Contains(ClassroomLink);
                    });
                }
                if (loggedIn && goToConfigLink) GoHome();
                return loggedIn;

            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine("Timed out: " + ex.Message);
            }
            return false;


        }
        public void GoHome()
        {
            if (string.IsNullOrWhiteSpace(config.Link))
            {
                throw new Exception(nameof(config.Link) + " is invalid");
            }
            driver.Navigate().GoToUrl(config.Link);
        }

        protected void SaveCookies(ReadOnlyCollection<Cookie> cookies, string cookiePath)
        {
            File.WriteAllText(cookiePath, JsonConvert.SerializeObject(cookies));
        }
        protected bool LoadCookies(string cookiePath)
        {
            if (!File.Exists(cookiePath)) return false;
            var dictArr = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(File.ReadAllText(cookiePath));
            var cookies = driver.Manage().Cookies;
            int addedCookies = 0;
            foreach (Dictionary<string, object> dict in dictArr)
            {
                cookies.AddCookie(Cookie.FromDictionary(dict));
                addedCookies++;
            }
            return addedCookies > 0;
        }

        public static void CreateEmpty<T>(string directory = "./") where T : Config, new()
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException("No such dir: " + directory);

            string path = Path.Combine(directory, "config.json");
            if (File.Exists(path))
                throw new Exception("File already exists: " + path);

            var meetConf = new T();
            File.WriteAllText(path, JsonConvert.SerializeObject(meetConf, Formatting.Indented));
        }

        public void Dispose()
        {
            driver.Dispose();
        }
    }
}