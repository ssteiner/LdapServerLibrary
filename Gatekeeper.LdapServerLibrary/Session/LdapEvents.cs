using Gatekeeper.LdapServerLibrary.Session.Events;
using Gatekeeper.LdapServerLibrary.Session.Replies;
using System.Threading.Tasks;

namespace Gatekeeper.LdapServerLibrary
{
    /// <summary>
    /// dummy implementation
    /// </summary>
    public class LdapEvents: ILdapEvents
    {

        /// <summary>
        /// abandons an ongoing search
        /// </summary>
        /// <param name="context"></param>
        /// <param name="abandonEvent"></param>
        /// <returns></returns>
        public Task OnAbandonRequest(ClientContext context, IAbandonEvent abandonEvent)
        {
            return Task.FromResult(0);
        }

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

    public interface ILdapEvents
    {
        /// <summary>
        /// authenticates requests
        /// </summary>
        /// <param name="context"></param>
        /// <param name="authenticationEvent"></param>
        /// <returns>Whether the authentication should succeed or not</returns>
        Task<bool> OnAuthenticationRequest(ClientContext context, IAuthenticationEvent authenticationEvent);

        /// <summary>
        /// performs the search request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="searchEvent"></param>
        /// <returns>List of search replies</returns>
        Task<SearchResultWrapper> OnSearchRequest(ClientContext context, ISearchEvent searchEvent);

        /// <summary>
        /// abandons an ongoing search
        /// </summary>
        /// <param name="context"></param>
        /// <param name="abandonEvent"></param>
        /// <returns></returns>
        Task OnAbandonRequest(ClientContext context, IAbandonEvent abandonEvent);
    }
}
