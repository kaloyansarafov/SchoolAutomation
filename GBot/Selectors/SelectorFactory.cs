using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace GBot
{
    public abstract class SelectorFactory
    {
        public abstract ReadOnlyDictionary<string, By> ForFirefox();
        public abstract ReadOnlyDictionary<string, By> ForChrome();
        public virtual ReadOnlyDictionary<string, By> Get(string browser)
        {
            switch (browser)
            {
                case "chrome":
                    return ForChrome();
                case "firefox":
                    return ForFirefox();
                default:
                    throw new System.NotSupportedException(browser);
            }
        }
    }
}