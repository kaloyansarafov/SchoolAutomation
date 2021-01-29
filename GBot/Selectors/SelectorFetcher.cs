using System;
using System.Collections.Generic;
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
        private HashSet<Type> TypesToTraverse;
        const int POST_DEPTH = 30;

        public SelectorFetcher(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, new TimeSpan(0, 0, 0, 0, 500));
            firstWait = new WebDriverWait(driver, new TimeSpan(0, 0, 4));
            logger = NLog.LogManager.GetCurrentClassLogger();
            TypesToTraverse = new();
        }
        public bool Register<T>()
        {
            return TypesToTraverse.Add(typeof(T));
        }
        /// <summary>
        /// Populates T object with the fetched FromSelectors
        /// </summary>
        /// <param name="baseSelector">Base selector of item. Used in relative searches.</param>
        /// <param name="isXpath">Are selectors xpath</param>
        /// <typeparam name="T">Class with FromSelector attributes</typeparam>
        /// <returns>Populated T item</returns>
        public T Fill<T>() where T : new()
        {
            var fromSel = typeof(T).GetCustomAttribute<FromSelector>();
            return Fill<T>(fromSel.Selector);
        }
        internal T Fill<T>(string baseSelector) where T : new()
        {
            bool isXpath = IsXpath(baseSelector);
            var selAttr = typeof(T).GetCustomAttribute<FromSelector>();
            T fill = new T();

            Queue<(object, string)> queue = new();
            queue.Enqueue((fill, baseSelector));
            while (queue.Count > 0)
            {
                (object @class, string baseSel) = queue.Dequeue();

                var props = @class.GetType().GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    var propSel = prop.GetCustomAttribute<FromXPath>();
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
                        if (TypesToTraverse.Contains(prop.PropertyType))
                        {
                            if (prop.PropertyType == @class.GetType())
                            {
                                logger.Debug("Recursion on {0}", @class.GetType().Name);
                                continue;
                            }
                            var ctor = prop.PropertyType.GetConstructor(System.Type.EmptyTypes);
                            if (ctor == null)
                                throw new Exception("No parameterless constructor for " + prop.PropertyType.Name);
                            object childClass = ctor.Invoke(null);
                            prop.SetValue(@class, childClass);
                            queue.Enqueue(
                                (childClass, baseSel + selector)
                            );
                        }
                        else
                        {
                            IWebElement el = Fetch(selector, isXpath);
                            if (!el.Displayed)
                            {
                                throw new ElementNotVisibleException();
                            }
                            object value = Parse(el, prop.PropertyType);
                            prop.SetValue(@class, value);
                        }
                    }
                    else
                    {
                        if (prop.PropertyType == typeof(IWebElement))
                        {
                            prop.SetValue(@class, Fetch(baseSel, isXpath));
                        }
                    }
                }
            }

            return fill;
        }

        private bool IsXpath(string baseSelector)
        {
            if (baseSelector.Length == 0)
            {
                throw new ArgumentException(nameof(baseSelector));
            }
            if (baseSelector.Trim()[0] == '/') return true;
            return false;
        }

        private IWebElement Fetch(string selector, bool isXpath)
        {
            WebDriverWait waiter = firstWait ?? wait;
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

        private object Parse(IWebElement el, Type propType)
        {
            object value = null;
            if (propType == typeof(string))
            {
                value = el.Text;
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
            //TODO REFACTOR
            T item = default(T);
            for (int i = 1; index >= 0;)
            {
                do
                {
                    string selector = baseClass.Selector.Replace("{index}", i.ToString());
                    try
                    {
                        item = Fill<T>(selector);
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
                    // logger.Trace("Filling selector [ {0} ]", selector);
                    try
                    {
                        el = Fill<T>(selector);
                    }
                    catch //(Exception ex)
                    {
                        el = default(T);
                    }
                    i++;
                } while (el == null && i < POST_DEPTH);
                if (el.Equals(item))
                {
                    found = true;
                }
                if (found)
                {
                    times--;
                }
            }
            return el;
        }
    }
}
