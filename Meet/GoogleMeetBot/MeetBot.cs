using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using GBot;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MeetGBot
{
    public sealed partial class MeetBot : Bot
    {
        readonly ReadOnlyDictionary<string, By> selectors;

        public MeetBot(Config config) : base(config)
        {
            selectors = new MeetSelectorFactory().Get(config.Driver.Browser);
        }
        public bool Login() => base.Login(goToConfigLink: false);

        readonly Regex meetRegex = new Regex(@"\/([a-z]{3,4}-?){3}");
        void Hangup()
        {
            IWebElement hangup = defaultWait.Until(driver =>
                driver.FindElement(
                    selectors[Elements.HangupButton]
                )
            );
            hangup.Click();
        }
        public bool InMeet()
        {
            if (!meetRegex.Match(driver.Url).Success)
            {
                return false;
            }
            try
            {
                IWebElement chat = driver.FindElement(selectors[Elements.ChatButton]);
                // Console.WriteLine("In meet with " + chat.Text);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            return true;
        }
        public void EnterMeet(string link)
        {
            driver.Navigate().GoToUrl(link);

            firstLoad.Until(driver => driver.Url == link);
            IWebElement microphone = defaultWait.Until(driver =>
            {
                // Console.WriteLine("Polling for mic");
                return driver.FindElement(selectors[Elements.MicrophoneButton]);
            }
            );
            userWait.Until(driver =>
            {
                // Console.WriteLine("Polling for mic enabled and displayed");
                return microphone.Enabled && microphone.Displayed;
            });
            string muted = microphone.GetAttribute("data-is-muted");
            if (bool.TryParse(muted, out bool result) && !result)
            {
                microphone.Click();
                // Console.WriteLine("Mic is muted");
            }

            IWebElement camera = driver.FindElement(selectors[Elements.CameraButton]);
            string hidden = camera.GetAttribute("data-is-muted");
            if (bool.TryParse(hidden, out result) && !result)
            {
                camera.Click();
                // Console.WriteLine("Camera is off");
            }

            IWebElement joinButton = driver.FindElement(selectors[Elements.JoinButton]);
            userWait.Until(driver => joinButton.Displayed);
            joinButton.Click();
        }
        public void LeaveMeet()
        {
            if (!InMeet())
            {
                throw new Exception("Not in meet");
            }
            Hangup();
        }

    }
}
