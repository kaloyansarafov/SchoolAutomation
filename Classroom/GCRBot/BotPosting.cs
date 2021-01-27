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

            return post;
        }
        public Post GetPostAfter(Post post, int times = 1)
        {
            if (!driver.Url.Contains("classroom.google.com"))
            {
                throw new Exception("Not at classroom url");
            }
            var find = selFetcher.FindAfter(post, times);
            return find;
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
            IWebElement el = defaultWait.Until(driver =>
                driver.FindElement(selectors[Elements.PostInput])
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