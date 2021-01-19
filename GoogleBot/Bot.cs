using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GoogleBot
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

        public void Dispose()
        {
            driver.Dispose();
        }
    }
}