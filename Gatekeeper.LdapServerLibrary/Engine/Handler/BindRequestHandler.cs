using System.Collections.Generic;
using System.Threading.Tasks;
using Gatekeeper.LdapServerLibrary.Models.Operations;
using Gatekeeper.LdapServerLibrary.Models.Operations.Response;
using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;
using Gatekeeper.LdapServerLibrary.Parser;
using Gatekeeper.LdapServerLibrary.Session.Events;
using System.Numerics;

namespace Gatekeeper.LdapServerLibrary.Engine.Handler
{
    internal class BindRequestHandler : IRequestHandler<BindRequest>
    {
        async Task<HandlerReply> IRequestHandler<BindRequest>.Handle(ClientContext context, ILdapEvents eventListener, BindRequest operation, BigInteger messageId)
        {
            Dictionary<string, List<string>> rdn = RdnParser.ParseRdnString(operation.Name);
            AuthenticationEvent authEvent = new AuthenticationEvent(rdn, operation.Authentication);
            bool success = await eventListener.OnAuthenticationRequest(context, authEvent);

            if (success)
            {
                if (rdn.Count == 0)
                    context.IsAnonymous = true;
                context.IsAuthenticated = true;
                context.Rdn = rdn;

                LdapResult ldapResult = new LdapResult(LdapResult.ResultCodeEnum.Success, null, null);
                BindResponse bindResponse = new BindResponse(ldapResult);
                return new HandlerReply(new List<IProtocolOp> { bindResponse });
            }
            else
            {
                context.IsAuthenticated = false;
                context.Rdn = new Dictionary<string, List<string>>();

                LdapResult ldapResult = new LdapResult(LdapResult.ResultCodeEnum.InappropriateAuthentication, null, null);
                BindResponse bindResponse = new BindResponse(ldapResult);
                return new HandlerReply(new List<IProtocolOp> { bindResponse });
            }
        }
    }
}
