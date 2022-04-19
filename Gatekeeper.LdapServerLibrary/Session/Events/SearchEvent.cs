using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;
using System.Numerics;

namespace Gatekeeper.LdapServerLibrary.Session.Events
{
    public class SearchEvent : ISearchEvent
    {
        public SearchRequest SearchRequest { get; set; } = null!;

        public BigInteger MessageId { get; set; }
    }
}
