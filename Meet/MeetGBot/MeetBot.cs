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
            logger.Info("Hanging up");
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
            logger.Info("Joining meet");
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
            IWebElement el = driver.FindElement(selectors[Elements.ChatButton]);
            logger.Debug("People in meet: " + el.Text);
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

            if (peopleInCall.Contains("No one else is here")) return 0;

            List<string> people = peopleInCall.Split(',').ToList();
            if (people.Count == 1)
            {
                return 1;
            }
            else //if (people.Count > 1)
            {
                //TODO FIX BUG
                string lastItem = people[people.Count - 1];
                people.Remove(lastItem);
                string[] split = lastItem.Split(" and ");
                if (split.Length > 2)
                {
                    throw new Exception("Wrong string: " + lastItem);
                }
                people.Add(split[0]);
                Regex numberReg = new Regex("[1-9]*");
                Match match = numberReg.Match(split[1]);
                if (match.Success)
                {
                    return people.Count + int.Parse(match.Value);
                }
                else
                {
                    return people.Count + 1;
                }
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
