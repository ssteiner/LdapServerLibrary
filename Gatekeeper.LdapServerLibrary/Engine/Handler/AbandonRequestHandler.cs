using System.Collections.Generic;
using System.Threading.Tasks;
using Gatekeeper.LdapServerLibrary.Models.Operations;
using Gatekeeper.LdapServerLibrary.Models.Operations.Response;
using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;
using Gatekeeper.LdapServerLibrary.Session.Events;
using System.Numerics;

namespace Gatekeeper.LdapServerLibrary.Engine.Handler
{
    internal class AbandonRequestHandler : IRequestHandler<AbandonRequest>
    {
        async Task<HandlerReply> IRequestHandler<AbandonRequest>.Handle(ClientContext context, ILdapEvents eventListener, AbandonRequest operation, BigInteger messageId)
        {
            await eventListener.OnAbandonRequest(context, new AbandonEvent { AbandonRequest = operation, MessageId = messageId });

            return new HandlerReply(new List<IProtocolOp> {
                new AbandonResponse()
            });
        }
    }
}
