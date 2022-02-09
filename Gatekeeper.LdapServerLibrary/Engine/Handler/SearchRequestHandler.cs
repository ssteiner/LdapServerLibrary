using System.Collections.Generic;
using System.Threading.Tasks;
using Gatekeeper.LdapServerLibrary.Models.Operations;
using Gatekeeper.LdapServerLibrary.Models.Operations.Response;
using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;
using Gatekeeper.LdapServerLibrary.Session.Events;
using Gatekeeper.LdapServerLibrary.Session.Replies;

namespace Gatekeeper.LdapServerLibrary.Engine.Handler
{
    internal class SearchRequestHandler : IRequestHandler<SearchRequest>
    {
        async Task<HandlerReply> IRequestHandler<SearchRequest>.Handle(ClientContext context, LdapEvents eventListener, SearchRequest operation)
        {
            SearchEvent searchEvent = new SearchEvent
            {
                SearchRequest = operation,
            };
            SearchResultWrapper replies = await eventListener.OnSearchRequest(context, searchEvent);

            List<IProtocolOp> opReply = new List<IProtocolOp>();
            if (replies.Results != null)
            {
                foreach (SearchResultReply reply in replies.Results)
                {
                    SearchResultEntry entry = new SearchResultEntry(reply);
                    opReply.Add(entry);
                }
            }

            var resultCode = (LdapResult.ResultCodeEnum)replies.ResultCode;

            LdapResult ldapResult = new LdapResult(resultCode, null, null);
            SearchResultDone searchResultDone = new SearchResultDone(ldapResult);
            opReply.Add(searchResultDone);

            return new HandlerReply(opReply);
        }
    }
}
