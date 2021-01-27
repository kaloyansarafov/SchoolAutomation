using GBot.Extensions;
using OpenQA.Selenium;

namespace GCRBot.Data
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
        public override string ToString()
        {
            int postedWord = Teacher.IndexOf("posted");
            string teacher = Teacher.Substring(0, postedWord);

            // Take only the assignment's name
            string name;
            if (Name.Contains("Assignment"))
            {
                int firstQuote = Name.IndexOf('"') + 1;
                int lastQuote = Name.LastIndexOf('"');
                name = Name.Substring(startIndex: firstQuote, length: lastQuote - firstQuote);
            }
            else
            {
                int firstQuote = Name.IndexOf('\'') + 1;
                int lastQuote = Name.LastIndexOf('\'');
                name = Name.Substring(startIndex: firstQuote, length: lastQuote - firstQuote);
            }

            return $"[ {Teacher}, {Timestamp}: {Name} ]";
        }
    }
}