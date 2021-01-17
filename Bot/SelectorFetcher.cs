using System;
using System.Reflection;
using GoogleCRBot.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GoogleCRBot
{
    internal class SelectorFetcher
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;
        private WebDriverWait firstWait;
        const int POST_DEPTH = 30;

        internal SelectorFetcher(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, new TimeSpan(0, 0, 0, 0, 500));
            firstWait = new WebDriverWait(driver, new TimeSpan(0, 0, 5));
        }
        /// <summary>
        /// Populates T object with the fetched FromSelectors
        /// </summary>
        /// <param name="baseSelector">Base selector of item. Used in relative searches.</param>
        /// <param name="isXpath">Are selectors xpath</param>
        /// <typeparam name="T">Class with certain attributes</typeparam>
        /// <returns>Populated T item</returns>
        private T get<T>(string baseSelector, bool isXpath) where T : new()
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

                    IWebElement el = fetchSelector(selector, isXpath);
                    if (!el.Displayed)
                    {
                        throw new ElementNotVisibleException();
                    }
                    object value = parse(el, prop.PropertyType);
                    prop.SetValue(toFill, value);
                }
                else
                {
                    if (prop.PropertyType == typeof(IWebElement))
                    {
                        prop.SetValue(toFill, fetchSelector(baseSelector, isXpath));
                    }
                }
            }

            return toFill;
        }

        private IWebElement fetchSelector(string selector, bool isXpath)
        {
            WebDriverWait waiter = firstWait ?? wait;
            IWebElement el = waiter.Until(driver =>
            {
                if (isXpath)
                {
                    return driver.FindElement(By.XPath(selector));
                }
                else
                {
                    return driver.FindElement(By.CssSelector(selector));
                }
            });
            if (firstWait != null) firstWait = null;
            return el;
        }

        object parse(IWebElement el, Type propType)
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
        /// Enumerates '{index}' in T
        /// </summary>
        /// <param name="index">Index from top of classroom.</param>
        /// <param name="found">
        /// Predicate used in searching for valid item.
        /// <br/>
        /// Default is item != null
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal T Find<T>(int index, Predicate<T> found = null) where T : new()
        {
            if (found == null)
            {
                found = item => item != null;
            }
            var type = typeof(T);
            FromSelector baseClass = type.GetCustomAttribute<FromSelector>();
            bool isXpath = baseClass is FromXPath;
            T item = default(T);
            for (int i = 1; index >= 0;)
            {
                do
                {
                    string selector = baseClass.Selector.Replace("{index}", i.ToString());
                    try
                    {
                        item = get<T>(selector, isXpath);
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
    }
}