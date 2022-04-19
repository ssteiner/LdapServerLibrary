using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;

namespace Gatekeeper.LdapServerLibrary.Session.Events
{
    public interface IAbandonEvent
    {
        AbandonRequest AbandonRequest { get; set; }
    }
}
