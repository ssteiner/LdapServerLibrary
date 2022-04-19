using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;
using System.Numerics;

namespace Gatekeeper.LdapServerLibrary.Session.Events
{
    internal class AbandonEvent : IAbandonEvent
    {
        public AbandonRequest AbandonRequest { get; set; } = null;

        public BigInteger MessageId { get; set; }

    }
}
