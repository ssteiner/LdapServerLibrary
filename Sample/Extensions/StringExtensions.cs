using System;

namespace Sample.Extensions
{
    public static class StringExtensions
    {
        public static string ValueBefore(this string source, string val, bool returnFullValueWhenValueNotFound = false)
        {
            if (val == null)
                throw new ArgumentException(nameof(val));
            if (source == null)
                return source;
            int index = source.IndexOf(val);
            if (index > 0)
                return source[..index];
            else if (returnFullValueWhenValueNotFound)
                return source;
            return string.Empty;
        }

        public static string ValueAfter(this string source, string val, bool returnFullValueWhenValueNotFound = false)
        {
            if (val == null)
                throw new ArgumentException(nameof(val));
            if (source == null)
                return source;
            int index = source.IndexOf(val);
            if (index > 0)
                return source[(index + 1)..];
            else if (source.StartsWith(val))
                return source[val.Length..];
            if (returnFullValueWhenValueNotFound)
                return source;
            return string.Empty;
        }
    }
}
