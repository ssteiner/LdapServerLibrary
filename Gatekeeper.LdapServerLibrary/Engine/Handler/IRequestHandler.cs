using System.Numerics;
using System.Threading.Tasks;
using Gatekeeper.LdapPacketParserLibrary.Models.Operations;

namespace Gatekeeper.LdapServerLibrary.Engine.Handler
{
    internal interface IRequestHandler<T> where T : IProtocolOp
    {
        internal Task<HandlerReply> Handle(ClientContext context, ILdapEvents eventListener, T operation, BigInteger messageId);
    }
}
