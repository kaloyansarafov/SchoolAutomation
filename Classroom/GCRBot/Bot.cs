using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using GCRBot.Data;
using GBot;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GCRBot
{
    public partial class ClassroomBot : Bot
    {
        SelectorFetcher selFetcher { get; }
        public ClassroomBot(Config config) : base(FixConfig(config))
        {
            selFetcher = new SelectorFetcher(driver);
        }
        static Config FixConfig(Config config)
        {
            config.Driver.Headless = true;
            return config;
        }
        public bool Login()
        {
            string path = Path.Combine("./", Cookies.GetName(config.Driver.Browser));
            if (!File.Exists(path))
            {
                Config loginConf = new Config();
                loginConf.Driver.Browser = config.Driver.Browser;
                loginConf.Driver.Headless = false;
                loginConf.Driver.DriverFolder = config.Driver.DriverFolder;
                LoginBot loginBot = null;
                try
                {
                    loginBot = new LoginBot(loginConf);

                    // Assumes user login
                    loginBot.Login();
                }
                finally
                {
                    loginBot?.Dispose();
                }
            }
            // Assumes cookies exist
            return base.Login();
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