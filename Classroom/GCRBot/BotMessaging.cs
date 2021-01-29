using System;
using System.Text.RegularExpressions;
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
                message.WebElement.FindElement(selectors[Elements.RelativeMessageInput])
            );
            el.SendKeys(text);
            logger.Trace($"Sending {text} to message");
            return el;
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
        public Message GetMessageAfter(Message message, int times = 1)
        {
            return selFetcher.FindAfter(message, times);
        }
        public bool WrittenCommentOn(Message message)
        {
            int amount = AmountOfComments(message);
            logger.Trace("Got amount of comments: {0}", amount);
            if (amount == 0)
            {
                return false;
            }
            if (amount == 1)
            {
                IWebElement comments = defaultWait.Until(driver =>
                    message.WebElement.FindElement(selectors[Elements.RelativeMessageComments])
                );
                IWebElement settings = comments.FindElement(By.XPath(".//div/div/div/div/div[1]/div[2]/div/div/div"));
                string label = settings.GetAttribute("aria-label");
                return IsOwnComment(label);
            }
            else
            {
                IWebElement comments = defaultWait.Until(driver =>
                    message.WebElement.FindElement(selectors[Elements.RelativeMessageComments])
                );
                comments.Click();
                for (int i = 1; i <= amount; i++)
                {
                    string fullSelector = $".//div[{i}]/div/div/div/div[1]/div[2]/div[1]";
                    IWebElement settings = comments.FindElement(By.XPath(fullSelector));
                    string label = settings.GetAttribute("aria-label");
                    logger.Trace("Got label {0} from '{1}'", label, fullSelector);
                    if (IsOwnComment(label)) return true;
                }
                return false;
            }
            throw new NotImplementedException();
        }
        private bool IsOwnComment(string label) => string.IsNullOrEmpty(label);
        private bool ShowMoreComments(Message message)
        {
            IWebElement el = defaultWait.Until(driver =>
                message.WebElement.FindElement(selectors[Elements.RelativeMessageCommentButton])
            );
            if (el.Enabled)
            {
                int amount = GetNumbersFrom(el.Text);
                if (amount > 1)
                {
                    el.Click();
                    return true;
                }
            }
            return false;
        }
        private int AmountOfComments(Message message)
        {
            IWebElement comments = defaultWait.Until(driver =>
                driver.FindElement(selectors[Elements.RelativeMessageComments])
            );
            try
            {
                firstLoad.Until(driver =>
                    comments.FindElement(By.XPath(".//div[last()]")).Enabled
                );
                IWebElement el = message.WebElement.FindElement(selectors[Elements.RelativeMessageCommentButton]);
                if (el.Enabled && el.Displayed)
                {
                    return GetNumbersFrom(el.Text);
                }
            }
            catch (Exception ex)
            {
                if (ex is NoSuchElementException || ex is WebDriverTimeoutException)
                {
                    return 0;
                }
                logger.Error(ex);
                throw;
            }
            logger.Error("Fuck");
            return 0;
        }
        private int GetNumbersFrom(string text)
        {
            Regex regex = new Regex("[0-9]*");
            return int.Parse(regex.Match(text).Value.Trim());
        }
    }
}