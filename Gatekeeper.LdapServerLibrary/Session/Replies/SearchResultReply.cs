using System.Collections.Generic;

namespace Gatekeeper.LdapServerLibrary.Session.Replies
{
    public class SearchResultReply
    {
        internal readonly string CommonName;
        internal readonly List<Attribute> Attributes;

        public SearchResultReply(
            string commonName,
            List<Attribute> attributes)
        {
            CommonName = commonName;
            Attributes = attributes;
        }

        public class Attribute
        {
            internal readonly string Key;
            internal List<string>? Values;
            internal byte[]? ByteValue;
            internal bool IsOid;

            public Attribute(string key, List<string> values, bool isOid = false)
            {
                Key = key;
                Values = values;
                IsOid = isOid;
            }

            public Attribute(string key, byte[] value)
            {
                Key = key;
                ByteValue = value;
            }
        }
    }

    public class SearchResultWrapper
    {
        public string? ErrorMessage { get; set; }

        public string? MatchedDn { get; set; }

        public int ResultCode { get; set; }

        public List<SearchResultReply>? Results { get; set; }
    }

}
