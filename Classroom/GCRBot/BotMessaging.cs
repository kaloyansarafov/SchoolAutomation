using System;
using GCRBot.Data;
using OpenQA.Selenium;

namespace GCRBot
{
    public partial class ClassroomBot
    {
        public void SendOnMessage(Message message, string text)
        {
            WriteOnMessage(message, text)
                .SendKeys(Keys.Tab + Keys.Enter);
        }
        internal IWebElement WriteOnMessage(Message message, string text)
        {
            UpdateFeed();
            IWebElement el = defaultWait.Until(driver =>
                message.WebElement.FindElement(By.XPath(".//div[2]/div/div[3]/div/div[2]/div/div/div/div[2]"))
            );
            el.SendKeys(text);
            logger.Trace($"Sending {text} to message");
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
                logger.Trace("Couldn't update feed...");
            }
        }
        /// <summary>
        /// Gets post from the top of the classroom
        /// </summary>
        /// <param name="index">Index of post</param>
        /// <returns></returns>
        public Message GetMessage(int index)
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
            // Get first post
            return selFetcher.Get<Message>(index);
        }
        public Message GetMessageAfter(Message message, int times)
        {
            return selFetcher.FindAfter(message, times);
        }
    }
}