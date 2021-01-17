using System;
using System.Collections.Generic;
using System.Reflection;
using GoogleCRBot.Data;
using OpenQA.Selenium;

namespace GoogleCRBot
{
    public partial class ClassroomBot
    {
        /// <summary>
        /// Gets post based on a selector map.
        /// <br/>
        /// Assumes page is not loading
        /// </summary>
        /// <param name="selectorMap">Keys should be the Post type's properties.</param>
        /// <returns>Populated Post object.</returns>
        internal Post getPost(Dictionary<string, string> selectorMap)
        {
            Post post = new Post();

            PropertyInfo[] props = typeof(Post).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                IWebElement el = FindXPathOrCss(selectorMap[prop.Name], driver);
                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(post, el.Text);
                }
            }

            return post;
        }
        internal bool tryGetPost(Dictionary<string, string> selectorMap, out Post post)
        {
            try
            {
                post = getPost(selectorMap);
            }
            catch
            {
                post = null;
                return false;
            }
            return true;
        }
        IWebElement FindXPathOrCss(string selector, IWebDriver driver)
        {
            try
            {
                return driver.FindElement(By.CssSelector(selector));
            }
            catch (OpenQA.Selenium.InvalidSelectorException)
            {
                return driver.FindElement(By.XPath(selector));
            }
        }
    }
}