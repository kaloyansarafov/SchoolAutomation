using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GBot;
using OpenQA.Selenium;

namespace MeetGBot
{
    internal static class SelectorFactory
    {
        static ReadOnlyDictionary<string, By> ForFirefox()
        {
            Dictionary<string, By> ffSels = new();
            ffSels.Add(Elements.CameraButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[1]/div[1]/div/div[3]/div[2]/div/div"));
            ffSels.Add(Elements.ChatButton,
                By.XPath("/html/body/div[1]/c-wiz/div[1]/div/div[7]/div[3]/div[6]/div[3]/div/div[2]/div[1]/span/span/div/div/span[2]"));
            ffSels.Add(Elements.HangupButton,
                By.XPath("/html/body/div[1]/c-wiz/div[1]/div/div[7]/div[3]/div[9]/div[2]/div[2]/div"));
            ffSels.Add(Elements.JoinButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[2]/div/div[2]/div/div[1]/div[1]/span/span"));
            ffSels.Add(Elements.MicrophoneButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[1]/div[1]/div/div[3]/div[1]/div/div/div"));
            return new ReadOnlyDictionary<string, By>(ffSels);
        }
        static ReadOnlyDictionary<string, By> ForChrome()
        {
            Dictionary<string, By> sels = new();
            sels.Add(Elements.CameraButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[8]/div[3]/div/div/div[2]/div/div[1]/div[1]/div[1]/div/div[3]/div[2]/div/div"));
            sels.Add(Elements.MicrophoneButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[8]/div[3]/div/div/div[2]/div/div[1]/div[1]/div[1]/div/div[3]/div[1]/div/div/div"));

            sels.Add(Elements.ChatButton,
                By.XPath("/html/body/div[1]/c-wiz/div[1]/div/div[8]/div[3]/div[6]/div[3]/div/div[2]/div[1]/span/span/div/div/span[2]"));
            sels.Add(Elements.HangupButton,
                By.XPath("/html/body/div[1]/c-wiz/div[1]/div/div[8]/div[3]/div[9]/div[2]/div[2]/div"));

            sels.Add(Elements.JoinButton,
                By.XPath("/html/body/div[1]/c-wiz/div/div/div[8]/div[3]/div/div/div[2]/div/div[1]/div[2]/div/div[2]/div/div[1]/div[1]/span/span"));

            return new ReadOnlyDictionary<string, By>(sels);
        }
        static internal ReadOnlyDictionary<string, By> Get(string browser)
        {
            switch (browser)
            {
                case "chrome":
                    return ForChrome();
                case "firefox":
                    return ForFirefox();
                default:
                    throw new NotSupportedException(browser);
            }
        }
    }
}