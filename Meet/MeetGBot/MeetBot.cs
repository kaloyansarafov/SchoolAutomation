using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GBot;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MeetGBot
{
    public enum MeetState
    {
        NotLoggedIn,
        InCall,
        InOverview,
        OutsideMeet
    }
    public sealed partial class MeetBot : Bot
    {
        private readonly ReadOnlyDictionary<string, By> selectors;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public MeetState State { get; private set; }

        private static Config FixConfig(Config config)
        {
            // Needs to be visible
            if (config.Driver.Headless) config.Driver.Headless = false;
            return config;
        }
        public MeetBot(Config config) : base(FixConfig(config))
        {
            selectors = new MeetSelectorFactory().Get(config.Driver.Browser);
            State = MeetState.NotLoggedIn;
        }

        public bool Login()
        {
            bool loggedIn = base.Login(goToConfigLink: false);
            if (loggedIn)
            {
                State = MeetState.OutsideMeet;
                logger.Trace("State change to " + State);
            }
            return loggedIn;
        }

        private void Hangup()
        {
            IWebElement hangup = defaultWait.Until(driver =>
                driver.FindElement(
                    selectors[Elements.HangupButton]
                )
            );
            logger.Debug("Hanging up");
            hangup.Click();
            State = MeetState.OutsideMeet;
            logger.Trace("State change to " + State);
        }

        public void EnterMeet()
        {
            if (State != MeetState.InOverview)
            {
                throw new Exception("Not in meet overview");
            }

            IWebElement joinButton = driver.FindElement(selectors[Elements.JoinButton]);
            userWait.Until(driver => joinButton.Displayed);
            logger.Debug("Joining meet");
            joinButton.Click();
            State = MeetState.InCall;
            logger.Trace("State change to " + State);
        }

        public void EnterMeetOverview(string link)
        {
            driver.Navigate().GoToUrl(link);
            if (link.Contains("/lookup/"))
            {
                logger.Trace("In lookup");
                Regex meetLinkReg = new Regex(@"https:\/\/meet.google.com\/([A-Za-z]{3}-?)([a-zA-Z]{4}-?)([A-Za-z]{3}-?)");
                firstLoad.Until(driver => meetLinkReg.Match(driver.Url).Success);
            }
            else firstLoad.Until(driver => driver.Url == link);

            State = MeetState.InOverview;
            logger.Trace("State change to " + State);

            MuteElement(Elements.MicrophoneButton);
            MuteElement(Elements.CameraButton);
        }
        public int PeopleInMeet()
        {
            if (State != MeetState.InCall)
            {
                throw new Exception("Not in meet call");
            }
            IWebElement el = firstLoad.Until(driver =>
                driver.FindElement(selectors[Elements.ChatButton])
            );
            return int.Parse(el.Text.Trim());
        }
        public int PeopleInMeetOverview()
        {
            if (State != MeetState.InOverview)
            {
                throw new Exception("Not in meet overview");
            }
            string peopleInCall = defaultWait.Until(driver =>
                driver.FindElement(selectors[Elements.PeopleInCallOverview])
            ).Text;

            if (peopleInCall.Contains("No one")) return 0;
            else if (peopleInCall.Contains(" is ")) return 1;

            List<string> split = peopleInCall.Split(", ").ToList();
            string[] andSplit = split[split.Count - 1].Split("and");

            split.Remove(split[split.Count - 1]);

            split.Add(andSplit[0]);
            Regex reg = new Regex("[0-9]*");
            var match = reg.Match(andSplit[1].Trim());
            if (!match.Success)
            {
                Console.WriteLine("Success");
                split.Add(andSplit[1]);
                return split.Count;
            }
            else
            {
                int val = int.Parse(match.Value);
                return split.Count + val;
            }
        }

        public void LeaveMeet()
        {
            if (State != MeetState.InCall)
            {
                throw new Exception("Not in meet");
            }
            Hangup();
        }

        private bool TryFindElement(By selector)
        {
            try
            {
                IWebElement el = driver.FindElement(selector);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            return true;
        }

        private void MuteElement(string element)
        {
            logger.Trace("Muting element " + element);
            IWebElement webElement = defaultWait.Until(driver =>
            {
                // Console.WriteLine("Polling for mic");
                return driver.FindElement(selectors[element]);
            });
            userWait.Until(driver =>
            {
                // Console.WriteLine("Polling for mic enabled and displayed");
                return webElement.Enabled && webElement.Displayed;
            });
            string muted = webElement.GetAttribute("data-is-muted");
            if (bool.TryParse(muted, out bool result) && !result)
            {
                webElement.Click();
                // Console.WriteLine("Mic is muted");
            }
        }
        public override void Dispose()
        {
            if (State == MeetState.InCall) Hangup();
            base.Dispose();
        }
    }
}
