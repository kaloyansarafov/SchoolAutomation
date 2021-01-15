using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenQA.Selenium;
using Newtonsoft.Json;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;
using System;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using GoogleCRBot.Data;

namespace GoogleCRBot
{
    public class ClassroomBot : IDisposable
    {
        BrowserData browserData { get; }
        internal GlobalData globalData { get; }
        IWebDriver driver { get; }
        WebDriverWait wait { get; }
        const string CONFIG = "config.json";
        const string FIRST_EL_XPATH = "//*[@id=\":1.t\"]";
        public ClassroomBot()
        {
            globalData = JsonConvert.DeserializeObject<GlobalData>(File.ReadAllText(CONFIG));
            driver = DriverFactory.InitDriver(globalData.DriverFolder, globalData.PreferredBrowser);
            browserData = DriverFactory.GetBrowserData(globalData);
            wait = new WebDriverWait(driver, new TimeSpan(0, 0, 5));
        }

        public void SendOnFirst(string message)
        {
            wait.Until(driver => driver.Navigate()).GoToUrl(globalData.ClassroomLink);
            IWebElement el = wait.Until(driver => driver.FindElement(By.XPath(FIRST_EL_XPATH)));
            el.SendKeys(message);
            el.SendKeys(Keys.Tab + Keys.Enter);
        }
        // Test method
        internal void WriteOnFirst(string message)
        {
            wait.Until(driver => driver.Navigate()).GoToUrl(globalData.ClassroomLink);
            IWebElement el = wait.Until(driver => driver.FindElement(By.XPath(FIRST_EL_XPATH)));
            el.SendKeys(message);
        }
        public bool Login()
        {
            try
            {
                wait.Until(driver => driver.Navigate()).GoToUrl("https://classroom.google.com/u/h");
                LoginTroughUser().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Got error: " + ex.Message);
                return false;
            }
            return true;
        }

        // Google default login screen
        private async Task LoginTroughUser()
        {
            (string username, string password) = globalData.User;

            // send username
            wait.Until(driver =>
                driver.FindElement(By.CssSelector("#identifierId"))
            ).SendKeys(username + Keys.Enter);

            // Delay
            await WaitFor((int)(browserData.Delays["login"]["afterUsername"] * 1000));

            // send password
            wait.Until(driver =>
                driver.FindElement(By.CssSelector("#password > div:nth-child(1) > div:nth-child(1) > div:nth-child(1) > input:nth-child(1)"))
            ).SendKeys(password + Keys.Enter);

            // Delay
            await WaitFor((int)(browserData.Delays["login"]["afterLogin"] * 1000));
        }
        async Task WaitFor(int ms)
        {
            Console.WriteLine("Delaying for " + ms);
            await Task.Delay(ms);
        }

        public void Dispose()
        {
            driver.Dispose();
        }
    }
}