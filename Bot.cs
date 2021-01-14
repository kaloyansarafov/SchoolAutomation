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
        GlobalData globalData { get; }
        IWebDriver driver { get; }
        WebDriverWait wait { get; }
        public ClassroomBot()
        {
            globalData = JsonConvert.DeserializeObject<GlobalData>(File.ReadAllText("data.json"));
            driver = BrowserFactory.InitDriver(globalData.DriverFolder, globalData.PreferredBrowser);
            browserData = BrowserFactory.GetBrowserData(globalData);
            wait = new WebDriverWait(driver, new TimeSpan(0, 0, 5));
        }

        public void SendOnFirst(string message, string url)
        {
            wait.Until(driver => driver.Navigate()).GoToUrl(url);
            IWebElement el = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\":1.t\"]")));
            el.SendKeys(message);
            el.SendKeys(Keys.Tab + Keys.Enter);
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

        private async Task LoginTroughUser()
        {
            (string username, string password) = GetCreds();

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

        (string, string) GetCreds()
        {
            IEnumerable<string> passwd = File.ReadLines("passwd.txt");
            return (passwd.ElementAt(0), passwd.ElementAt(1));
        }

        public void Dispose()
        {
            driver.Dispose();
        }
    }
}