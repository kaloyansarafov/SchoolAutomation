using System;
using GCRBot.Data;
using OpenQA.Selenium;

namespace GCRBot
{
    public partial class ClassroomBot
    {
        public Post GetPost(int index)
        {
            if (!driver.Url.Contains("classroom.google.com"))
            {
                throw new Exception("Not at classroom url");
            }
            UpdateFeed();
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            // Take only the teacher's name
            var post = selFetcher.Get<Post>(index,
                item => item != null && !string.IsNullOrEmpty(item.Teacher));

            return Parse(post);
        }
        public Post GetPostAfter(Post post, int times)
        {
            if (!driver.Url.Contains("classroom.google.com"))
            {
                throw new Exception("Not at classroom url");
            }
            var find = selFetcher.FindAfter(post, times);
            return Parse(find);
        }

        private static Post Parse(Post post)
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
            if (!driver.Url.Contains("classroom.google.com"))
            {
                throw new Exception("Not at classroom url");
            }
            post.WebElement.Click();
        }
        internal IWebElement WriteOnCurrentPost(string message)
        {
            IWebElement el = defaultWait.Until(driver => driver.FindElement(
                By.XPath(@"/html/body/div[2]/div/div/div[2]/div[2]/div[9]/div/div/div[4]/div/div[2]/div[1]/div/div/div[2]"))
            );
            firstLoad.Until(driver => el.Displayed);
            el.SendKeys(message);
            return el;
        }
        public void SendOnCurrentPost(string message)
        {
            if (!driver.Url.Contains("classroom.google.com"))
            {
                throw new Exception("Not at classroom url");
            }
            IWebElement el = WriteOnCurrentPost(message);
            el.SendKeys(Keys.Tab + Keys.Enter);
        }
    }
}