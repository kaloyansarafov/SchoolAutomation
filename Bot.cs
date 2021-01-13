using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenQA.Selenium;
using Newtonsoft.Json;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;
using System;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;

namespace GoogleCRBot
{
    public class ClassroomBot : IDisposable
    {
        Data data { get; init; }
        IWebDriver driver;
        StreamWriter logger;
        WebDriverWait wait;
        public ClassroomBot()
        {
            data = JsonConvert.DeserializeObject<Data>(File.ReadAllText("data.json"));
            logger = new StreamWriter(data.LogFolder + "/" + DateTime.Now.ToString("d-M-yyyy_h-mm-ss") + ".txt");
            InitDriver();
            wait = new WebDriverWait(driver, new TimeSpan(0, 0, 10));
        }
        void InitDriver()
        {
            try
            {
                Console.WriteLine("Attaching webdriver");
                driver = new RemoteWebDriver(new Uri(data.RemoteURI), new FirefoxOptions());
            }
            catch (OpenQA.Selenium.WebDriverException)
            {
                Console.WriteLine("Starting webdriver");
                StartWebDriver();

                // Try to reatach
                driver = new RemoteWebDriver(new Uri(data.RemoteURI), new FirefoxOptions());
            }
            // catch (InvalidOperationException)
            // {
            // Could restart web driver
            // }
            Console.WriteLine("Attached webdriver");
        }
        private void StartWebDriver()
        {
            string filename = data.DriverFolder + "/geckodriver.exe";
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("Download 'geckodriver' and put it in the drivers folder");
            }
            Process proc = new Process();
            proc.StartInfo.FileName = filename;
            proc.StartInfo.RedirectStandardError = true;

            proc.ErrorDataReceived += WriteLogs;

            if (!proc.Start())
            {
                throw new OperationCanceledException("Couldn't start webdriver");
            }
        }

        private void WriteLogs(object sender, DataReceivedEventArgs e)
        {
            var proc = sender as Process;
            logger.WriteLine($"\nError: {proc.StandardError.ReadToEnd()}\n");
            logger.Flush();
        }

        public void SendOnFirst(string message, string url)
        {
            logger.WriteLine(url);
            wait.Until(driver => driver.Navigate()).GoToUrl(url);
            IWebElement el = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\":1.t\"]")));
            el.SendKeys(message);
            el.SendKeys(Keys.Tab + Keys.Enter);
        }
        public bool Login()
        {
            try
            {
                wait.Until(driver => driver.Navigate()).GoToUrl("https://classroom.google.com/u/h");
                LoginTroughUser().Wait();
            }
            catch (Exception ex)
            {
                logger.WriteLine("Got error: " + ex.Message);
                return false;
            }
            return true;
        }

        private async Task LoginTroughUser()
        {
            (string username, string password) = GetCreds();
            wait.Until(driver =>
                driver.FindElement(By.CssSelector("#identifierId"))
            ).SendKeys(username + Keys.Enter);

            await WaitFor(data.Delays["login"]["afterUsername"]);

            wait.Until(driver =>
                driver.FindElement(By.CssSelector("#password > div:nth-child(1) > div:nth-child(1) > div:nth-child(1) > input:nth-child(1)"))
            ).SendKeys(password + Keys.Enter);

            await WaitFor(data.Delays["login"]["afterLogin"]);
        }
        async Task WaitFor(int ms)
        {
            Console.WriteLine("Delaying for " + ms);
            await Task.Delay(ms);
        }

        (string, string) GetCreds()
        {
            IEnumerable<string> passwd = File.ReadLines("passwd.txt");
            return (passwd.ElementAt(0), passwd.ElementAt(1));
        }

        public void Dispose()
        {
            driver.Dispose();
            logger.Close();
            logger.Dispose();
        }
    }
}