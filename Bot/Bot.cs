using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using GoogleCRBot.Data;

namespace GoogleCRBot
{
    public partial class ClassroomBot : IDisposable
    {
        internal Config config { get; }
        IWebDriver driver { get; }
        WebDriverWait wait { get; }
        SelectorFetcher selFetcher { get; }
        public ClassroomBot(Config config)
        {
            this.config = config;
            driver = DriverFactory.InitDriver(config.Driver);
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
            Console.WriteLine(message.WebElement.Text);
            IWebElement el = wait.Until(driver =>
                message.WebElement.FindElement(By.XPath(".//div[2]/div/div[3]/div/div[2]/div/div/div/div[2]"))
            );
            el.SendKeys(text);
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
            return selFetcher.Get<Message>(index);
        }
        public Message GetMessageAfter(Message message, int times)
        {
            return selFetcher.FindAfter(message, times);
        }
        public Post GetPost(int index)
        {
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
            driver.Navigate().GoToUrl(config.ClassroomLink);
        }

        public bool Login()
        {
            wait.Until(driver => driver.Navigate()).GoToUrl(config.ClassroomLink);
            LoginTroughUser();
            int lastSlash = config.ClassroomLink.LastIndexOf('/');
            string crID = config.ClassroomLink[lastSlash..];
            bool loggedIn = wait.Until(driver =>
                driver.Url.Contains(crID)
            );
            return loggedIn;
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
        public void Dispose()
        {
            driver.Dispose();
        }
    }
}