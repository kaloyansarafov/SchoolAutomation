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
    public partial class ClassroomBot : IDisposable
    {
        BrowserConfig browserConf { get; }
        internal Config config { get; }
        IWebDriver driver { get; }
        // Timeout
        WebDriverWait wait { get; }
        const string CONFIG = "config.json";
        public ClassroomBot()
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG));
            driver = DriverFactory.InitDriver(config.DriverFolder, config.PreferredBrowser);
            browserConf = DriverFactory.GetBrowserData(config);
            wait = new WebDriverWait(driver, new TimeSpan(0, 0, 5));
        }

        public void SendOnPost(string message, int index)
        {
            if (index < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            WriteOnPost(message, index)
                .SendKeys(Keys.Tab + Keys.Enter);
        }
        internal IWebElement WriteOnPost(string message, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            IWebElement el = wait.Until(driver =>
                driver.FindElement(By.XPath("//*[@id=\":1.t\"]".Replace("1", (index + 1).ToString())))
            );
            el.SendKeys(message);
            return el;
        }
        /// <summary>
        /// Gets the post at the index from the top of the posts in the classroom
        /// </summary>
        /// <param name="index">Should be atleast zero</param>
        /// <returns></returns>
        public Post GetPost(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            // Could make into data class which holds selectors and such
            Dictionary<string, string> selectorMap = new();

            // Add all of post's properties to map
            var props = typeof(Post).GetProperties();
            foreach (var prop in props)
            {
                selectorMap.Add(prop.Name, "");
            }

            // Get first post
            Post post = findPost(index, selectorMap);
            return post;
        }

        /// <summary>
        /// Searches trough posts from the top of the page.
        /// </summary>
        /// <param name="index">Post index.</param>
        /// <param name="selectorMap">Map with keys matching Post type's props</param>
        /// <returns></returns>
        private Post findPost(int index, Dictionary<string, string> selectorMap)
        {
            // Max 10 posts back
            if (index < 0 || index > 10) throw new ArgumentOutOfRangeException(nameof(index));

            string posts = "/html/body/div[2]/div/div[2]/main/section/div/div[2]";
            Post post = null;
            for (int i = 1; index >= 0;)
            {
                bool found = false;
                do
                {
                    string selected = $"{posts}/div[{i}]";

                    selectorMap["Name"] = $"{selected}/div[1]/div[1]/div[1]/div/div/span";
                    selectorMap["Timestamp"] = $"{selected}/div[1]/div[1]/div[1]/span";
                    selectorMap["Message"] = $"{selected}/div[1]/div[2]/div[1]/html-blob/span";

                    found = tryGetPost(selectorMap, out post);
                    i++;
                } while (!found && i < 10);
                index--;
            }
            return post;
        }

        public async Task<bool> Login()
        {
            try
            {
                wait.Until(driver => driver.Navigate()).GoToUrl("https://classroom.google.com/u/h");
                await LoginTroughUser();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Got error: " + ex.Message);
                return false;
            }
            driver.Navigate().GoToUrl(config.ClassroomLink);
            if (driver.Url != config.ClassroomLink)
            {
                return false;
            }
            await WaitFor(browserConf.Delays["classroom"]["onLoad"]);
            return true;
        }

        // Google default login screen
        private async Task LoginTroughUser()
        {
            (string username, string password) = config.User;

            // send username
            wait.Until(driver =>
                driver.FindElement(By.CssSelector("#identifierId"))
            ).SendKeys(username + Keys.Enter);

            // Delay
            await WaitFor(browserConf.Delays["login"]["afterUsername"]);

            // send password
            wait.Until(driver =>
                driver.FindElement(By.CssSelector("#password > div:nth-child(1) > div:nth-child(1) > div:nth-child(1) > input:nth-child(1)"))
            ).SendKeys(password + Keys.Enter);

            // Delay
            await WaitFor(browserConf.Delays["login"]["afterLogin"]);
        }
        async Task WaitFor(float seconds)
        {
            if (seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            int ms = (int)(seconds * 1000);
            Console.WriteLine("Delaying for " + ms + " milliseconds.");
            await Task.Delay(ms);
            Console.WriteLine("Done");
        }

        public void Dispose()
        {
            driver.Dispose();
        }
    }
}