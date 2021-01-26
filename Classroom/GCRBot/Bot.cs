using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using GCRBot.Data;
using GBot;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using NLog;

namespace GCRBot
{
    public partial class ClassroomBot : Bot
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        SelectorFetcher selFetcher { get; }
        public ClassroomBot(Config config) : base(config)
        {
            selFetcher = new SelectorFetcher(driver);
        }
        public bool Login()
        {
            string path = Path.Combine("./", Cookies.GetName(config.Driver.Browser));
            logger.Debug("Cookie path: " + path);
            if (!File.Exists(path))
            {
                logger.Trace("Launching login bot");
                Config loginConf = new Config();
                loginConf.Driver.Browser = config.Driver.Browser;
                loginConf.Driver.Headless = false;
                loginConf.Driver.DriverFolder = config.Driver.DriverFolder;
                LoginBot loginBot = null;
                try
                {
                    loginBot = new LoginBot(loginConf);

                    // Assumes user login
                    bool loggedIn = loginBot.Login();
                    if (!loggedIn) logger.Debug("LoginBot failed");
                }
                finally
                {
                    loginBot?.Dispose();
                }
            }
            logger.Trace("On to base login");
            // Assumes cookies exist
            return base.Login(goToConfigLink: true);
        }
    }
    class LoginBot : Bot
    {
        public LoginBot(Config config) : base(config)
        {

        }
        public bool Login()
        {
            return base.Login(goToConfigLink: false);
        }
    }
}