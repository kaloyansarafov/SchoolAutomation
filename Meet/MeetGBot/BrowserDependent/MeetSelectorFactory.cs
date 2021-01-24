using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GBot;
using OpenQA.Selenium;

namespace MeetGBot
{
    internal class MeetSelectorFactory : SelectorFactory
    {
        public override ReadOnlyDictionary<string, By> ForFirefox()
        {
            Dictionary<string, By> selectors = new();
            selectors.Add(Elements.CameraButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[1]/div[1]/div/div[3]/div[2]/div/div"));
            selectors.Add(Elements.ChatButton,
                By.XPath("/html/body/div[1]/c-wiz/div[1]/div/div[7]/div[3]/div[6]/div[3]/div/div[2]/div[1]/span/span/div/div/span[2]"));
            selectors.Add(Elements.HangupButton,
                By.XPath("/html/body/div[1]/c-wiz/div[1]/div/div[7]/div[3]/div[9]/div[2]/div[2]/div"));
            selectors.Add(Elements.JoinButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[2]/div/div[2]/div/div[1]/div[1]/span/span"));
            selectors.Add(Elements.MicrophoneButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[1]/div[1]/div/div[3]/div[1]/div/div/div"));
            selectors.Add(Elements.PeopleInCallOverview,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[2]/div/div[1]/div[1]/div[2]/div[2]"));
            selectors.Add(Elements.ReadyToJoinMessage,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[2]/div/div[1]/div[1]/div[1]/div"));
            return new ReadOnlyDictionary<string, By>(selectors);
        }
        public override ReadOnlyDictionary<string, By> ForChrome()
        {
            Dictionary<string, By> selectors = new();
            selectors.Add(Elements.CameraButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[8]/div[3]/div/div/div[2]/div/div[1]/div[1]/div[1]/div/div[3]/div[2]/div/div"));
            selectors.Add(Elements.MicrophoneButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[8]/div[3]/div/div/div[2]/div/div[1]/div[1]/div[1]/div/div[3]/div[1]/div/div/div"));

            selectors.Add(Elements.ChatButton,
                By.XPath("/html/body/div[1]/c-wiz/div[1]/div/div[8]/div[3]/div[6]/div[3]/div/div[2]/div[1]/span/span/div/div/span[2]"));
            selectors.Add(Elements.HangupButton,
                By.XPath("/html/body/div[1]/c-wiz/div[1]/div/div[8]/div[3]/div[9]/div[2]/div[2]/div"));

            selectors.Add(Elements.JoinButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[8]/div[3]/div/div/div[2]/div/div[1]/div[2]/div/div[2]/div/div[1]/div[1]/span/span"));

            selectors.Add(Elements.PeopleInCallOverview,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[8]/div[3]/div/div/div[2]/div/div[1]/div[2]/div/div[1]/div[1]/div[2]/div[2]"));

            selectors.Add(Elements.ReadyToJoinMessage,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[8]/div[3]/div/div/div[2]/div/div[1]/div[2]/div/div[1]/div[1]/div[1]/div"));

            return new ReadOnlyDictionary<string, By>(selectors);
        }
    }
}