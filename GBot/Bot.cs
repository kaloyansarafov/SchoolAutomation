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
        static readonly string ClassroomLink = $"https://{CLASSROOM_URI}/";
        static readonly string EduLink = $"https://{EDU_URI}";

        protected readonly string CookiesPath;

        protected Config config { get; }
        protected IWebDriver driver { get; init; }
        public Bot(Config config)
        {
            driver = DriverFactory.InitDriver(config.Driver);
            this.config = config;

            CookiesPath = Cookies.GetPath(config.Driver.PreferredBrowser);

            defaultWait = new WebDriverWait(driver, new TimeSpan(0, 0, seconds: 10));
            firstLoad = new WebDriverWait(driver, new TimeSpan(0, 0, seconds: 15));
            userWait = new WebDriverWait(driver, new TimeSpan(0, minutes: 1, 0));
        }

        public virtual bool Login()
        {
            defaultWait.Until(driver => driver.Navigate()).GoToUrl(EduLink);
            firstLoad.Until(driver => driver.Url.Contains(EDU_URI));

            bool loadedCookies = LoadCookies(CookiesPath);

            driver.Navigate().GoToUrl(LoginLink);

            //TODO Replace with factory method
            bool firefox = config.Driver.PreferredBrowser == "firefox";

            // if(firefox) checks:
            //   firefox can't insert google classroom cookies into edu.google.com
            try
            {
                bool loggedIn = false;
                if (!loadedCookies)
                {
                    loggedIn = userWait.Until(driver => driver.Url.Contains(ClassroomLink));
                    if (loggedIn)
                    {
                        driver.Navigate().GoToUrl(EduLink);

                        // Save edu.google.com cookies insted of classroom's
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
        public void GoHome()
        {
            driver.Navigate().GoToUrl(LoginLink);
        }

        // Google default login screen
        //TODO Deprecate
        protected void LoginByCredentials(string username, string password)
        {
            // send username
            IWebElement identifier = userWait.Until(driver =>
                driver.FindElement(By.CssSelector("#identifierId"))
            );
            userWait.Until(driver => identifier.Displayed);
            identifier.SendKeys(username + Keys.Enter);
            // send password
            IWebElement passwordEl = userWait.Until(driver =>
            {
                return driver.FindElement(
                    By.XPath("//*[@id=\"password\"]/div[1]/div/div[1]/input")
                );
            });
            userWait.Until(driver => passwordEl.Displayed);
            passwordEl.SendKeys(password + Keys.Enter);
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