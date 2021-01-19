using GoogleBot.Extensions;
using OpenQA.Selenium;

namespace GoogleCRBot.Data
{
    [FromXPath("/html/body/div[2]/div/div[2]/main/section/div/div[2]/div[{index}]/div[1]/div/div[3]")]
    public record Post
    {
        [FromXPath("/div/div/span")]
        public string Teacher { get; init; }
        [FromXPath("/span/span[2]")]
        public string Timestamp { get; init; }
        [FromXPath("/div/h2/span")]
        public string Name { get; init; }
        public IWebElement WebElement { get; init; }
    }
}