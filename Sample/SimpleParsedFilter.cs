using System.Collections.Generic;

namespace Sample
{
    public class SimpleParsedFilter
    {

        public List<AttributeFilter> AttributeFilters { get; set; } = new List<AttributeFilter>();

    }

    public class AttributeFilter
    {
        public Operator Operator { get; set; }

        public string Attribute { get; set; }

        public string AttributeValue { get; set; }
    }

    public enum Operator { Equals, Substring, GreaterOrEqual, LessOrEqual }
}
