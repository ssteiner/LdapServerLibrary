using System.Collections.Generic;
using System.Threading.Tasks;
using Gatekeeper.LdapServerLibrary.Models.Operations;
using Gatekeeper.LdapServerLibrary.Models.Operations.Response;
using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;
using System.Numerics;

namespace Gatekeeper.LdapServerLibrary.Engine.Handler
{
    internal class UnbindRequestHandler : IRequestHandler<UnbindRequest>
    {
        Task<HandlerReply> IRequestHandler<UnbindRequest>.Handle(ClientContext context, ILdapEvents eventListener, UnbindRequest operation, BigInteger messageId)
        {
            context.IsAuthenticated = false;
            context.Rdn = new Dictionary<string, List<string>>();
            return Task.FromResult(new HandlerReply(new List<IProtocolOp> {
                new UnbindDummyResponse()
            }));
        }
    }
}
