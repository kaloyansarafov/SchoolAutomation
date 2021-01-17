using System;
namespace GoogleCRBot.Extensions
{
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Class,
                            Inherited = false, AllowMultiple = false)]
    public abstract class FromSelector : System.Attribute
    {
        public string Selector { get; }
        public bool InheritFromClass { get; }

        public FromSelector(string selector, bool inheritFromClass = true)
        {
            Selector = selector;
            InheritFromClass = inheritFromClass;
        }
    }
    public class FromXPath : FromSelector
    {
        public FromXPath(string selector, bool inheritFromClass = true) : base(selector, inheritFromClass)
        {
        }
    }
    public class FromCss : FromSelector
    {
        public FromCss(string selector, bool inheritFromClass = true) : base(selector, inheritFromClass)
        {
        }
    }
}