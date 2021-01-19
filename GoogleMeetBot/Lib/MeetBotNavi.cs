using System;
using OpenQA.Selenium;

namespace GoogleMeetBot
{
    public partial class MeetBot
    {
        void Hangup()
        {
            IWebElement hangup = wait.Until(driver =>
                driver.FindElement(By.XPath("/html/body/div[1]/c-wiz/div[1]/div/div[7]/div[3]/div[9]/div[2]/div[2]/div"))
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
                IWebElement people = driver.FindElement(By.XPath("/html/body/div[1]/c-wiz/div[1]/div/div[7]/div[3]/div[6]/div[3]/div/div[2]/div[1]/span/span/div/div/span[2]"));
                Console.WriteLine("In meet with " + people.Text);
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
            IWebElement microphone = firstLoad.Until(driver =>
                driver.FindElement(By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[1]/div[1]/div/div[3]/div[1]/div/div/div"))
            );
            string muted = microphone.GetAttribute("data-is-muted");
            Console.WriteLine("Mic is muted: " + muted);
            if (!bool.Parse(muted))
            {
                microphone.Click();
                Console.WriteLine("Mic is muted: " + microphone.GetAttribute("data-is-muted"));
            }

            IWebElement camera = driver.FindElement(By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[1]/div[1]/div/div[3]/div[2]/div/div"));
            string hidden = camera.GetAttribute("data-is-muted");
            Console.WriteLine("Camera is off: " + hidden);
            if (!bool.Parse(hidden))
            {
                camera.Click();
                Console.WriteLine("Camera is off: " + camera.GetAttribute("data-is-muted"));
            }

            //TODO Wait for button to showup
            // \/ \/ \/ \/ \/ \/ \/ Crashes
            IWebElement joinButton = driver.FindElement(By.XPath("/html/body/div[1]/c-wiz/div/div/div[7]/div[3]/div/div/div[2]/div/div[1]/div[2]/div/div[2]/div/div[1]/div[1]"));
            wait.Until(driver => joinButton.Displayed && joinButton.Enabled);
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