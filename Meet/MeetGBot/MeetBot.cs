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
            if (loggedIn) State = MeetState.OutsideMeet;
            return loggedIn;
        }

        private void Hangup()
        {
            IWebElement hangup = defaultWait.Until(driver =>
                driver.FindElement(
                    selectors[Elements.HangupButton]
                )
            );
            hangup.Click();
        }

        public void EnterMeet()
        {
            if (State != MeetState.InOverview)
            {
                throw new Exception("Not in meet overview");
            }

            IWebElement joinButton = driver.FindElement(selectors[Elements.JoinButton]);
            userWait.Until(driver => joinButton.Displayed);
            joinButton.Click();
            State = MeetState.InCall;
        }

        public void EnterMeetOverview(string link)
        {
            driver.Navigate().GoToUrl(link);
            if (link.Contains("/lookup/"))
            {
                Regex meetLinkReg = new Regex(@"https:\/\/meet.google.com\/([A-Za-z]{3}-?)([a-zA-Z]{4}-?)([A-Za-z]{3}-?)");
                firstLoad.Until(driver => meetLinkReg.Match(driver.Url).Success);
            }
            else firstLoad.Until(driver => driver.Url == link);

            State = MeetState.InOverview;

            MuteElement(selectors[Elements.MicrophoneButton]);
            MuteElement(selectors[Elements.CameraButton]);
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
                Console.WriteLine(people[0] + " is alone.");
                return 1;
            }
            else //if (people.Count > 1)
            {
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
            State = MeetState.OutsideMeet;
        }

        private bool TryFindElement(By selector)
        {
            try
            {
                IWebElement readyToJoinMessage = driver.FindElement(selector);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            return true;
        }

        private void MuteElement(By selector)
        {
            IWebElement webElement = defaultWait.Until(driver =>
            {
                // Console.WriteLine("Polling for mic");
                return driver.FindElement(selector);
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
    }
}
