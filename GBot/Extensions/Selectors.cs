using System;
namespace GBot.Extensions
{
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Class,
                            Inherited = false, AllowMultiple = false)]
    public abstract class FromSelector : System.Attribute
    {
        public string Selector { get; }
        public bool InheritFromClass { get; }

        public FromSelector(string selector, bool inherit = true)
        {
            Selector = selector;
            InheritFromClass = inherit;
        }
    }
    public class FromXPath : FromSelector
    {
        public FromXPath(string selector, bool inherit = true) : base(selector, inherit)
        {
        }
    }
    public class FromCss : FromSelector
    {
        public FromCss(string selector, bool inherit = true) : base(selector, inherit)
        {
        }
    }
}