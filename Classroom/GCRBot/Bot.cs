using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using GCRBot.Data;
using GBot;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GCRBot
{
    public partial class ClassroomBot : Bot
    {
        SelectorFetcher selFetcher { get; }
        WebDriverWait wait;
        const string cookiePath = "cookies.json";
        public ClassroomBot(CRConfig config) : base(config)
        {
            this.driver = DriverFactory.InitDriver(this.config.Driver);
            SetCredentials(config.User.Username, config.User.Password);
            wait = new WebDriverWait(driver, new TimeSpan(0, 0, 7));
            selFetcher = new SelectorFetcher(driver);
        }
        public void SendOnMessage(Message message, string text)
        {
            WriteOnMessage(message, text)
                .SendKeys(Keys.Tab + Keys.Enter);
        }
        internal IWebElement WriteOnMessage(Message message, string text)
        {
            UpdateFeed();
            IWebElement el = wait.Until(driver =>
                message.WebElement.FindElement(By.XPath(".//div[2]/div/div[3]/div/div[2]/div/div/div/div[2]"))
            );
            el.SendKeys(text);
            return el;
        }
        void UpdateFeed()
        {
            try
            {
                IWebElement el = driver.FindElement(By.XPath("/html/body/nav/div[3]/div[2]/div/div/div/div"));
                if (el.Displayed)
                {
                    el.Click();
                }
            }
            catch (NoSuchElementException)
            {
                // Do nothing.
            }
        }
        /// <summary>
        /// Gets post from the top of the classroom
        /// </summary>
        /// <param name="index">Index of post</param>
        /// <returns></returns>
        public Message GetMessage(int index)
        {
            UpdateFeed();
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            // Get first post
            return selFetcher.Get<Message>(index);
        }
        public Message GetMessageAfter(Message message, int times)
        {
            return selFetcher.FindAfter(message, times);
        }
        public Post GetPost(int index)
        {
            UpdateFeed();
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            // Take only the teacher's name
            var post = selFetcher
                                .Get<Post>(index,
                                    item => item != null && !string.IsNullOrEmpty(item.Teacher)
                                );
            return parse(post);
        }
        public Post GetPostAfter(Post post, int times)
        {
            var find = selFetcher.FindAfter(post, times);
            return parse(find);
        }

        private static Post parse(Post post)
        {
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

        public void GoToPost(Post post)
        {
            post.WebElement.Click();
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
            driver.Navigate().GoToUrl(config.Link);
        }

        (string username, string password) Credentials;
        public void SetCredentials(string username, string password)
        {
            Credentials = (username, password);
        }
        static string classroomLink = "https://classroom.google.com/";
        public override bool Login()
        {
            if (string.IsNullOrEmpty(Credentials.username)
                || string.IsNullOrEmpty(Credentials.password))
            {
                throw new Exception("No set credentials");
            }
            wait.Until(driver => driver.Navigate()).GoToUrl(classroomLink);
            bool loaded = LoadCookies();
            wait.Until(driver => driver.Navigate()).GoToUrl(config.Link);

            if (!loaded)
            {
                LoginByCredentials(Credentials.username, Credentials.password);
            }

            int lastSlash = config.Link.LastIndexOf('/');
            string crID = config.Link[lastSlash..];
            bool loggedIn = wait.Until(driver =>
                driver.Url.Contains(crID)
            );
            if (!loaded) SaveCookies(driver.Manage().Cookies.AllCookies);
            return loggedIn;
        }
        void SaveCookies(ReadOnlyCollection<Cookie> cookies)
        {
            File.WriteAllText(cookiePath, JsonConvert.SerializeObject(cookies));
        }
        bool LoadCookies()
        {
            if (!File.Exists(cookiePath)) return false;
            var dictArr = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(File.ReadAllText(cookiePath));
            var cookies = driver.Manage().Cookies;
            int addedCookies = 0;
            foreach (Dictionary<string, object> dict in dictArr)
            {
                Console.WriteLine("Adding cookie");
                cookies.AddCookie(Cookie.FromDictionary(dict));
                addedCookies++;
            }
            return addedCookies > 0;
        }

    }
}