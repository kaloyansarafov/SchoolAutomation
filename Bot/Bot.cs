using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium;
using Newtonsoft.Json;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;
using System;
using GoogleCRBot.Data;

namespace GoogleCRBot
{
    public partial class ClassroomBot : IDisposable
    {
        internal Config config { get; }
        IWebDriver driver { get; }
        // Timeout
        WebDriverWait wait { get; }
        SelectorFetcher selFetcher { get; }
        const string CONFIG = "config.json";
        public ClassroomBot()
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG));
            driver = DriverFactory.InitDriver(config.DriverFolder, config.PreferredBrowser);
            // ITimeouts timeouts = driver.Manage().Timeouts();
            // timeouts.PageLoad = new TimeSpan(0, 0, 2);
            // timeouts.ImplicitWait = new TimeSpan(0, 0, 2);
            wait = new WebDriverWait(driver, new TimeSpan(0, 0, 5));
            selFetcher = new SelectorFetcher(driver);
        }
        public void SendOnMessage(string message, int index)
        {
            if (index < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            WriteOnMessage(message, index)
                .SendKeys(Keys.Tab + Keys.Enter);
        }
        internal IWebElement WriteOnMessage(string message, int index)
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
        /// Gets post from the top of the classroom
        /// </summary>
        /// <param name="index">Index of post</param>
        /// <returns></returns>
        public Message GetMessage(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            // Get first post
            return selFetcher.Find<Message>(index);
        }
        public Post GetPostOverview(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            // Take only the teacher's name
            var post = selFetcher
                                .Find<Post>(index,
                                    item => item != null && !string.IsNullOrEmpty(item.Teacher)
                                );
            int postedWord = post.Teacher.IndexOf("posted");
            string teacher = post.Teacher.Substring(0, postedWord);

            // Take only the assignment's name
            string name;
            if (post.Name.Contains("Assignment"))
            {
                int firstQuote = post.Name.IndexOf('"') + 1;
                int lastQuote = post.Name.LastIndexOf('"');
                name = post.Name.Substring(startIndex: firstQuote, length: lastQuote - firstQuote);
            }
            else
            {
                int firstQuote = post.Name.IndexOf('\'') + 1;
                int lastQuote = post.Name.LastIndexOf('\'');
                name = post.Name.Substring(startIndex: firstQuote, length: lastQuote - firstQuote);
            }

            return new Post()
            {
                Teacher = teacher,
                Timestamp = post.Timestamp,
                Name = name,
                WebElement = post.WebElement
            };
        }
        public Post GoToPost(int index)
        {
            Post item = GetPostOverview(index);
            item.WebElement.Click();
            return item;
        }
        internal IWebElement WriteOnCurrentPost(string message)
        {
            IWebElement el = wait.Until(driver => driver.FindElement(
                By.XPath(@"/html/body/div[2]/div/div/div[2]/div[2]/div[9]/div/div/div[4]/div/div[2]/div[1]/div/div/div[2]"))
            );
            wait.Until(driver => el.Displayed);
            el.SendKeys(message);
            return el;
        }
        public void SendOnCurrentPost(string message)
        {
            IWebElement el = WriteOnCurrentPost(message);
            el.SendKeys(Keys.Tab + Keys.Enter);
        }

        public void GoHome()
        {
            driver.Navigate().GoToUrl(config.ClassroomLink);
        }

        public bool Login()
        {
            wait.Until(driver => driver.Navigate()).GoToUrl(config.ClassroomLink);
            LoginTroughUser();
            return true;
        }

        // Google default login screen
        private void LoginTroughUser()
        {
            (string username, string password) = config.User;

            // send username
            IWebElement identifier = wait.Until(driver =>
                driver.FindElement(By.CssSelector("#identifierId"))
            );
            wait.Until(driver => identifier.Displayed);
            identifier.SendKeys(username + Keys.Enter);
            // send password
            IWebElement passwordEl = wait.Until(driver =>
            {
                return driver.FindElement(
                    By.XPath("//*[@id=\"password\"]/div[1]/div/div[1]/input")
                );
            });
            wait.Until(driver => passwordEl.Displayed);
            passwordEl.SendKeys(password + Keys.Enter);
        }
        private void WaitFor(float seconds)
        {
            Task.Delay((int)(seconds * 1000)).Wait();
        }
        public void Dispose()
        {
            driver.Dispose();
        }
    }
}