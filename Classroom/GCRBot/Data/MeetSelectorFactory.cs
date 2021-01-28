using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GCRBot.Data;
using OpenQA.Selenium;

namespace GCRBot
{
    internal static class MeetSelectorFactory
    {
        private static Dictionary<string, By> ForFirefox()
        {
            Dictionary<string, By> selectors = new();
            selectors.Add(Elements.PostInput,
                By.XPath(@"/html/body/div[2]/div/div/div[2]/div[2]/div[9]/div/div/div[4]/div/div[2]/div[1]/div/div/div[2]"));
            selectors.Add(Elements.RelativeMessageInput,
                By.XPath(".//div[2]/div/div[3]/div/div[2]/div/div/div/div[2]"));
            selectors.Add(Elements.ShowMoreButton,
                By.XPath("/html/body/nav/div[3]/div[2]/div/div/div/div"));
            selectors.Add(Elements.RelativeMessageCommentButton,
                By.XPath(".//div[2]/div/div[1]"));
            selectors.Add(Elements.RelativeMessageComments,
                By.XPath(".//div[2]/div/div[2]"));
            selectors.Add(Elements.ClassroomMeetLink,
                By.XPath("/html/body/div[2]/div/div[1]/div/div[2]/div[2]/div/span/a"));
            return selectors;
        }
        private static Dictionary<string, By> ForChrome()
        {
            Dictionary<string, By> chrome = ForFirefox();
            chrome[Elements.ClassroomMeetLink] = By.XPath("/html/body/div[2]/div/div[1]/div/div[2]/div[2]/div/span/a");
            return chrome;
        }
        public static ReadOnlyDictionary<string, By> Get(string browser)
        {
            switch (browser)
            {
                case "firefox":
                    return new ReadOnlyDictionary<string, By>(ForFirefox());
                case "chrome":
                    return new ReadOnlyDictionary<string, By>(ForChrome());
                default:
                    throw new NotSupportedException(nameof(browser));
            }
        }
    }
}