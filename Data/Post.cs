using OpenQA.Selenium;

namespace GoogleCRBot.Data
{
    public record Post
    {
        public string Name { get; init; }
        public string Timestamp { get; init; }
        public string Message { get; init; }
    };
}