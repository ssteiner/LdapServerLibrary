using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;
using Gatekeeper.LdapServerLibrary;
using Gatekeeper.LdapServerLibrary.Session.Events;
using Gatekeeper.LdapServerLibrary.Session.Replies;
using Microsoft.Extensions.Logging;
using Sample.Extensions;

namespace Sample
{
    class LdapEventListener : ILdapEvents
    {

        private bool allowAnonymous;
        private readonly ILogger logger;
        private readonly UserDatabase db;
        private readonly List<UserDatabase.User> users;
        private readonly IQueryable<UserDatabase.User> usersQuery;

        public LdapEventListener(ILogger logger, bool allowAnonymous = false)
        {
            this.allowAnonymous = allowAnonymous;
            this.logger = logger;
            db = new UserDatabase();
            users = db.GetUserDatabase();
            usersQuery = users.AsQueryable();
        }

        public Task<bool> OnAuthenticationRequest(ClientContext context, IAuthenticationEvent authenticationEvent)
        {
            authenticationEvent.Rdn.TryGetValue("cn", out List<string> cnValue);
            authenticationEvent.Rdn.TryGetValue("dc", out List<string> dcValue);

            if ((cnValue == null || cnValue.Count == 0) && string.IsNullOrEmpty(authenticationEvent.Password) && allowAnonymous)
                return Task.FromResult(true);
            var userDn = $"{string.Join(",", cnValue.Select(x => $"cn={x}"))},{string.Join(",", dcValue.Select(x => $"dc={x}"))}";
            logger.Log(LogLevel.Information, $"Login from {userDn}");
            if (cnValue.Contains("Manager") && dcValue.Contains("example") && dcValue.Contains("com"))
            {
                logger.Log(LogLevel.Information, "Login allowed");
                return Task.FromResult(true);
            }
            else if (cnValue.Contains("OnlyBindUser") && authenticationEvent.Password == "OnlyBindUserPassword")
            {
                logger.Log(LogLevel.Information, "bind-only login allowed");
                return Task.FromResult(true);
            }
            logger.Log(LogLevel.Warning, "Login denied");
            return Task.FromResult(false);
        }

        private readonly string serverRoot = "dc=example,dc=com";

        public async Task<SearchResultWrapper> OnSearchRequest(ClientContext context, ISearchEvent searchEvent)
        {
            //Console.WriteLine(Convert.ToBase64String(searchEvent.SearchRequest.RawPacket));
            if (context.Rdn.TryGetValue("cn", out var cns) && cns.FirstOrDefault() == "OnlyBindUser")
            {
                return new SearchResultWrapper { ResultCode = 32 }; // return 'no such object
            }
            if (searchEvent.SearchRequest.Filter is SearchRequest.PresentFilter presentFilter) // extracting a single item / path
            {
                return ProcessPresentFilter(searchEvent, presentFilter, context);
            }
            else // it's a search
            {
                return await ProcessSearchRequest(searchEvent, context).ConfigureAwait(false);
            }
        }

        private SearchResultWrapper ProcessPresentFilter(ISearchEvent searchEvent, SearchRequest.PresentFilter presentFilter, ClientContext context)
        {
            var resultWrapper = new SearchResultWrapper { };
            if (searchEvent.SearchRequest.BaseObject == string.Empty && presentFilter.Value?.ToLower() == LdapUtil.ObjectClass)
            {
                var directories = LdapUtil.ListAvailableDirectories(searchEvent.SearchRequest.BaseObject, serverRoot, DirectoryNames, 
                    searchEvent.SearchRequest.AttributeSelection);
                if (searchEvent.SearchRequest.AttributeSelection?.Contains(LdapUtil.NamingContexts, StringComparer.OrdinalIgnoreCase) == true) // list naming contexts
                {
                    resultWrapper.Results = new List<SearchResultReply> { LdapUtil.GetRoot(serverRoot, searchEvent.SearchRequest.AttributeSelection) };
                }
                else if (searchEvent.SearchRequest.AttributeSelection?.Contains(LdapUtil.SubSchemaSubEntry, StringComparer.OrdinalIgnoreCase) == true) // subschema subentries => not supported
                {
                    resultWrapper.Results = new List<SearchResultReply>();
                }
                else
                    resultWrapper.Results = directories;
                return resultWrapper;
            }
            else if (!string.IsNullOrEmpty(searchEvent.SearchRequest.BaseObject))
            {
                var baseObjectPath = searchEvent.SearchRequest.BaseObject?.ToLower();
                if (string.Compare(baseObjectPath, serverRoot, StringComparison.OrdinalIgnoreCase) == 0 
                    && presentFilter.Value?.ToLower() == LdapUtil.ObjectClass) // search root of directory
                {
                    if (searchEvent.SearchRequest.Scope == SearchRequest.ScopeEnum.SingleLevel) // list all items
                        resultWrapper.Results = LdapUtil.ListAvailableDirectories(searchEvent.SearchRequest.BaseObject, serverRoot, DirectoryNames, 
                            searchEvent.SearchRequest.AttributeSelection);
                    else if (searchEvent.SearchRequest.Scope == SearchRequest.ScopeEnum.BaseObject) // get the root itself
                        resultWrapper.Results = new List<SearchResultReply> { LdapUtil.GetRoot(serverRoot, searchEvent.SearchRequest.AttributeSelection) };
                    //var allUsers = DumpUsers(db.GetUserDatabase());
                    //resultWrapper.Results.AddRange(allUsers);
                    return resultWrapper;
                }
                else if (baseObjectPath?.EndsWith(serverRoot, StringComparison.OrdinalIgnoreCase) == true && presentFilter.Value?.ToLower() == LdapUtil.ObjectClass) // list a subdirectory
                {
                    if (LdapUtil.IsDirectoryFolder(baseObjectPath, DirectoryNames, serverRoot)) // path is a directory folder
                    {
                        var directoryDn = baseObjectPath.ValueBefore(serverRoot).ValueAfter("cn=").TrimEnd(',');
                        if (searchEvent.SearchRequest.Scope == SearchRequest.ScopeEnum.BaseObject) // get the object
                        {
                            var directory = LdapUtil.GenerateDirectory(directoryDn, serverRoot, searchEvent.SearchRequest.AttributeSelection);
                            resultWrapper.Results = new List<SearchResultReply> { directory };
                        }
                        else if (searchEvent.SearchRequest.Scope == SearchRequest.ScopeEnum.SingleLevel) // expand the object
                        {
                            var directoryUsers = usersQuery.Where(u => u.Dn.ToLower().EndsWith(baseObjectPath));
                            var parsedUsers = DumpUsers(directoryUsers);
                            resultWrapper.Results = new List<SearchResultReply>(parsedUsers);
                        }
                        return resultWrapper;
                    }
                    else if (LdapUtil.IsObjectInDirectory(baseObjectPath, DirectoryNames, serverRoot, true)) // object that is in a directory folder or the root
                    {
                        var allUsers = users.Where(u => u.Dn.ToLower() == searchEvent.SearchRequest.BaseObject?.ToLower()).ToList();
                        var listReply = DumpUsers(allUsers);
                        if (searchEvent.SearchRequest.Scope == SearchRequest.ScopeEnum.BaseObject) // get the object
                            resultWrapper.Results = listReply;
                        return resultWrapper; // libs makes an object not found out of this.. it should return an OK since
                    }
                    else // unknown path
                    {
                        return new SearchResultWrapper { ResultCode = 32 }; // return 'no such object
                    }
                }
                else // get single item
                {
                    var allUsers = users.Where(u => u.Dn.ToLower() == searchEvent.SearchRequest.BaseObject?.ToLower()).ToList();
                    var listReply = DumpUsers(allUsers);
                    if (searchEvent.SearchRequest.AttributeSelection.All(x => x.ToLower() != LdapUtil.ObjectClass)) // just check if it's available
                        resultWrapper.Results = listReply;
                    return resultWrapper; // libs makes an object not found out of this.. it should return an OK since
                }
            }
            return resultWrapper;
        }

        private Task<SearchResultWrapper> ProcessSearchRequest(ISearchEvent searchEvent, ClientContext context)
        {
            var resultWrapper = new SearchResultWrapper { };
            var filter = LdapUtil.RestoreLdapFilter(searchEvent.SearchRequest.Filter);

            int? limit = searchEvent.SearchRequest.SizeLimit;

            // Load the user database that queries will be executed against
            //UserDatabase dbContainer = new UserDatabase();
            //IQueryable<UserDatabase.User> userDb = dbContainer.GetUserDatabase().AsQueryable();

            var itemExpression = Expression.Parameter(typeof(UserDatabase.User));
            var searchExpressionBuilder = new SearchExpressionBuilder(searchEvent);
            var conditions = searchExpressionBuilder.Build(searchEvent.SearchRequest.Filter, itemExpression);
            var queryLambda = Expression.Lambda<Func<UserDatabase.User, bool>>(conditions, itemExpression);
            var predicate = queryLambda.Compile();

            var results = usersQuery.Where(predicate).ToList();

            var replies = new List<SearchResultReply>();
            foreach (var user in results)
            {
                var attributes = new List<SearchResultReply.Attribute>();
                var reply = new SearchResultReply(
                    user.Dn,
                    attributes
                );

                foreach (KeyValuePair<string, List<string>> attribute in user.Attributes)
                {
                    var attributeClass = new SearchResultReply.Attribute(attribute.Key, attribute.Value);
                    attributes.Add(attributeClass);
                }

                replies.Add(reply);
            }

            resultWrapper.Results = replies;
            return Task.FromResult(resultWrapper);
        }

        private List<SearchResultReply> DumpUsers(IEnumerable<UserDatabase.User> users)
        {
            var replies = new List<SearchResultReply>();
            foreach (var user in users)
            {
                var attributes = new List<SearchResultReply.Attribute>();
                var reply = new SearchResultReply(user.Dn, attributes);
                foreach (var attribute in user.Attributes)
                {
                    var attributeClass = new SearchResultReply.Attribute(attribute.Key, attribute.Value);
                    attributes.Add(attributeClass);
                }
                if (user.ByteAttributes != null)
                {
                    foreach (var attribute in user.ByteAttributes)
                    {
                        var attributeClass = new SearchResultReply.Attribute(attribute.Key, attribute.Value);
                        attributes.Add(attributeClass);
                    }
                }
                replies.Add(reply);
            }
            return replies;
        }

        private readonly List<string> DirectoryNames = new() { "ActiveDirectory", "Zuweiser", "ETV.online" };

    }
}
