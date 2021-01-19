using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace GoogleBot
{
    internal class DriverWithExe : IWebDriver
    {
        public string Browser { get; }
        public string Url { get => _driver.Url; set => _driver.Url = value; }

        public string Title => _driver.Title;

        public string PageSource => _driver.PageSource;

        public string CurrentWindowHandle => _driver.CurrentWindowHandle;

        public ReadOnlyCollection<string> WindowHandles => _driver.WindowHandles;

        IWebDriver _driver;
        Process _driverExe;

        internal void Connect(Uri uri, DriverOptions options)
        {
            _driver = new RemoteWebDriver(uri, options);
        }
        internal void StartDriverExe(string driverPath)
        {
            if (!File.Exists(driverPath))
            {
                throw new FileNotFoundException($"No such file: {driverPath}");
            }
            Process proc = new Process();
            proc.StartInfo.FileName = driverPath;

            if (!proc.Start())
            {
                throw new OperationCanceledException($"Couldn't start {driverPath}");
            }
            _driverExe = proc;
        }

        public void Close()
        {
            _driver.Close();
        }

        public void Quit()
        {
            _driver.Quit();
        }

        public IOptions Manage()
        {
            return _driver.Manage();
        }

        public INavigation Navigate()
        {
            return _driver.Navigate();
        }

        public ITargetLocator SwitchTo()
        {
            return _driver.SwitchTo();
        }

        public IWebElement FindElement(By by)
        {
            return _driver.FindElement(by);
        }

        public ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            return _driver.FindElements(by);
        }

        public void Dispose()
        {
            _driverExe?.Close();
            _driverExe?.Dispose();
            _driver.Dispose();
        }
    }
}