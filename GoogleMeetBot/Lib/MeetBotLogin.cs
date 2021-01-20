using System;
using System.Collections.Generic;
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
        readonly WebDriverWait userWait;

        readonly WebDriverWait wait;
        readonly Regex meetRegex;

        const string crCookies = "crCookies.json";
        const string meetCookies = "meetCookies.json";
        static string loginLink = "https://classroom.google.com/u/";
        static string classroomLink = "https://classroom.google.com/";

        private bool loginWithCookies = false;

        public MeetBot(MeetConfig config) : base(config)
        {
            driver = DriverFactory.InitDriver(config.Driver);
            meetRegex = new Regex(@"\/([a-z]{3,4}-?){3}");

            wait = new WebDriverWait(this.driver, new TimeSpan(0, 0, seconds: 10));
            firstLoad = new WebDriverWait(driver, new TimeSpan(0, 0, seconds: 15));
            userWait = new WebDriverWait(driver, new TimeSpan(0, minutes: 1, 0));
        }
        public void GoHome()
        {
            driver.Navigate().GoToUrl(loginLink);
        }

        public override bool Login()
        {
            wait.Until(driver => driver.Navigate()).GoToUrl(classroomLink);
            wait.Until(driver => driver.Url == classroomLink);
            bool loaded = LoadCookies(crCookies);
            wait.Until(driver => driver.Navigate()).GoToUrl(loginLink);
            try
            {
                bool loggedIn = false;
                if (!loaded)
                {
                    loggedIn = userWait.Until(driver => driver.Url == loginLink);
                    if (loggedIn) SaveCookies(driver.Manage().Cookies.AllCookies, crCookies);
                }
                else
                {
                    loggedIn = firstLoad.Until(driver =>
                    {
                        return driver.Url.Contains("classroom.google.com");
                    });
                    loginWithCookies = loaded && loggedIn;
                }
                return loggedIn;
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine(ex.Source);
            }
            return false;
        }

    }
}
