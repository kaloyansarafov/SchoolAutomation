using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GBot
{
    public abstract class Bot : IDisposable
    {
        protected Config config { get; }
        protected IWebDriver driver { get; init; }
        protected TimeSpan loginTimeout { get; set; }
        public Bot(Config config)
        {
            this.config = config;
            loginTimeout = new TimeSpan(0, 0, 10);
        }

        public abstract bool Login();

        // Google default login screen
        protected void LoginByCredentials(string username, string password)
        {
            WebDriverWait loginWait = new WebDriverWait(driver, loginTimeout);

            // send username
            IWebElement identifier = loginWait.Until(driver =>
                driver.FindElement(By.CssSelector("#identifierId"))
            );
            loginWait.Until(driver => identifier.Displayed);
            identifier.SendKeys(username + Keys.Enter);
            // send password
            IWebElement passwordEl = loginWait.Until(driver =>
            {
                return driver.FindElement(
                    By.XPath("//*[@id=\"password\"]/div[1]/div/div[1]/input")
                );
            });
            loginWait.Until(driver => passwordEl.Displayed);
            passwordEl.SendKeys(password + Keys.Enter);
        }
        protected void SaveCookies(ReadOnlyCollection<Cookie> cookies, string cookiePath)
        {
            File.WriteAllText(cookiePath, JsonConvert.SerializeObject(cookies));
        }
        protected bool LoadCookies(string cookiePath)
        {
            if (!File.Exists(cookiePath)) return false;
            var dictArr = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(File.ReadAllText(cookiePath));
            var cookies = driver.Manage().Cookies;
            int addedCookies = 0;
            foreach (Dictionary<string, object> dict in dictArr)
            {
                cookies.AddCookie(Cookie.FromDictionary(dict));
                addedCookies++;
            }
            return addedCookies > 0;
        }

        public void Dispose()
        {
            driver.Dispose();
        }
    }
}