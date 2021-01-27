﻿using System;
using System.Reflection;
using GBot.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GBot
{
    public class SelectorFetcher
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;
        private WebDriverWait firstWait;
        private NLog.Logger logger;
        const int POST_DEPTH = 30;

        public SelectorFetcher(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, new TimeSpan(0, 0, 0, 0, 500));
            firstWait = new WebDriverWait(driver, new TimeSpan(0, 0, 4));
            logger = NLog.LogManager.GetCurrentClassLogger();
        }
        /// <summary>
        /// Populates T object with the fetched FromSelectors
        /// </summary>
        /// <param name="baseSelector">Base selector of item. Used in relative searches.</param>
        /// <param name="isXpath">Are selectors xpath</param>
        /// <typeparam name="T">Class with certain attributes</typeparam>
        /// <returns>Populated T item</returns>
        internal T Fill<T>(string baseSelector, bool isXpath) where T : new()
        {
            T toFill = new T();

            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                FromSelector propSel = prop.GetCustomAttribute<FromSelector>();
                if (propSel != null)
                {
                    string selector = propSel.Selector;
                    if (propSel.InheritFromClass)
                    {
                        if (isXpath && propSel is not FromXPath)
                        {
                            throw new InvalidSelectorException("Class and properties have different types");
                        }
                        selector = baseSelector + selector;
                    }

                    IWebElement el = Fetch(selector, isXpath);
                    if (!el.Displayed)
                    {
                        throw new ElementNotVisibleException();
                    }
                    object value = Parse(el, prop.PropertyType);
                    prop.SetValue(toFill, value);
                }
                else
                {
                    if (prop.PropertyType == typeof(IWebElement))
                    {
                        prop.SetValue(toFill, Fetch(baseSelector, isXpath));
                    }
                }
            }

            return toFill;
        }

        public IWebElement Fetch(string selector, bool isXpath)
        {
            WebDriverWait waiter = firstWait ?? wait;
            logger.Trace($"Fetching {selector} for {waiter.Timeout}");
            IWebElement el;
            if (isXpath)
            {
                el = waiter.Until(driver =>
                   driver.FindElement(By.XPath(selector))
                );
            }
            else
            {
                el = waiter.Until(driver =>
                  driver.FindElement(By.CssSelector(selector))
                );
            }
            if (firstWait != null) firstWait = null;
            return el;
        }

        object Parse(IWebElement el, Type propType)
        {
            object value = null;
            if (propType == typeof(string))
            {
                value = el.Text;
            }
            else if (propType == typeof(IWebElement))
            {
                value = el;
            }

            return value;
        }


        /// <summary>
        /// Enumerates from top of classroom: '{index}' in T's selectors
        /// </summary>
        /// <param name="index">Index from top of classroom.</param>
        /// <param name="found">
        /// Predicate used in searching for valid item.
        /// <br/>
        /// Default is item != null
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(int index, Predicate<T> found = null) where T : new()
        {
            if (found == null)
            {
                found = item => item != null;
            }
            var type = typeof(T);
            FromSelector baseClass = type.GetCustomAttribute<FromSelector>();
            bool isXpath = baseClass is FromXPath;
            //TODO REFACTOR
            T item = default(T);
            for (int i = 1; index >= 0;)
            {
                do
                {
                    string selector = baseClass.Selector.Replace("{index}", i.ToString());
                    try
                    {
                        item = Fill<T>(selector, isXpath);
                    }
                    catch //(Exception ex)
                    {
                        item = default(T);
                    }
                    i++;
                } while (!found(item) && i < POST_DEPTH);
                index--;
            }

            return item;
        }
        public T FindAfter<T>(T item, int times) where T : new()
        {
            T el = default(T);
            FromSelector baseClass = typeof(T).GetCustomAttribute<FromSelector>();
            bool isXpath = baseClass is FromXPath;
            int i = 1;
            bool found = false;
            logger.Trace("Received {0}", item);
            while (times >= 0)
            {
                do
                {
                    string selector = baseClass.Selector.Replace("{index}", i.ToString());
                    logger.Trace("Filling selector [ {0} ]", selector);
                    try
                    {
                        el = Fill<T>(selector, isXpath);
                    }
                    catch //(Exception ex)
                    {
                        el = default(T);
                    }
                    i++;
                } while (el == null && i < POST_DEPTH);
                logger.Trace("Found element: {0}", el);
                if (el.Equals(item))
                {
                    found = true;
                    logger.Trace("{0} matches {1}", el, item);
                }
                if (found)
                {
                    logger.Trace("Found, counting off: {0}", times);
                    times--;
                }
            }
            return el;
        }
    }
}
