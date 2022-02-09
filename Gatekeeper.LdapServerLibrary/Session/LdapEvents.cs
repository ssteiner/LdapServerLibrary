using Gatekeeper.LdapServerLibrary.Session.Events;
using Gatekeeper.LdapServerLibrary.Session.Replies;
using System.Threading.Tasks;

namespace Gatekeeper.LdapServerLibrary
{
    public class LdapEvents
    {
        /// <summary>
        /// Override this for authentication requests.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="authenticationEvent"></param>
        /// <returns>Whether the authentication should succeed or not</returns>
        public virtual Task<bool> OnAuthenticationRequest(ClientContext context, IAuthenticationEvent authenticationEvent)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Override this for search request support.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="searchEvent"></param>
        /// <returns>List of search replies</returns>
        public virtual Task<SearchResultWrapper> OnSearchRequest(ClientContext context, ISearchEvent searchEvent)
        {
            return Task.FromResult(new SearchResultWrapper());
        }
    }
}
