using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using GoogleBot;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GoogleMeetBot
{
    public partial class MeetBot : Bot
    {
        readonly WebDriverWait firstLoad;

        private readonly WebDriverWait wait;
        readonly Regex meetRegex;
        ReadOnlyCollection<Cookie> Cookies;
        const string cookiePath = "/home/lyubenk/source/repos/GoogleCRBot/GoogleMeetBot/Automation/cookies.json";
        private static string loginLink = "https://classroom.google.com/u/";
        public MeetBot(MeetConfig config) : base(config)
        {
            driver = DriverFactory.InitDriver(config.Driver);
            wait = new WebDriverWait(this.driver, new TimeSpan(0, 0, seconds: 10));
            firstLoad = new WebDriverWait(driver, new TimeSpan(0, 0, seconds: 15));
            meetRegex = new Regex(@"\/([a-z]{3,4}-?){3}");
            SetCredentials(config.User.Username, config.User.Password);
        }
        public void GoHome()
        {
            driver.Navigate().GoToUrl(loginLink);
        }

        public override bool Login()
        {
            if (string.IsNullOrEmpty(Credentials.username)
                || string.IsNullOrEmpty(Credentials.password))
            {
                throw new Exception("No set credentials");
            }
            wait.Until(driver => driver.Navigate()).GoToUrl(loginLink);
            LoginByCredentials(Credentials.username, Credentials.password);
            try
            {
                return wait.Until(driver =>
                   driver.Url == loginLink
                );
            }
            catch (WebDriverTimeoutException)
            {
            }
            return false;
        }

        (string username, string password) Credentials;
        public void SetCredentials(string username, string password)
        {
            Credentials = (username, password);
        }
    }
}
