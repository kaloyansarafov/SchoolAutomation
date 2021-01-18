using GoogleCRBot.Extensions;
using OpenQA.Selenium;

namespace GoogleCRBot.Data
{
    [FromXPath("/html/body/div[2]/div/div[2]/main/section/div/div[2]/div[{index}]")]
    public record Message
    {
        [FromXPath("/div[1]/div[1]/div[1]/div/div/span")]
        public string Teacher { get; init; }
        [FromXPath("/div[1]/div[1]/div[1]/span/span[2]")]
        public string Timestamp { get; init; }
        [FromXPath("/div[1]/div[2]/div[1]/html-blob/span")]
        public string Information { get; init; }
        public IWebElement WebElement { get; init; }
    };
}