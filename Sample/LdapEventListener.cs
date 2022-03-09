using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;
using Gatekeeper.LdapServerLibrary;
using Gatekeeper.LdapServerLibrary.Session.Events;
using Gatekeeper.LdapServerLibrary.Session.Replies;
using Sample.Extensions;

namespace Sample
{
    class LdapEventListener : ILdapEvents
    {

        private bool allowAnonymous;

        public LdapEventListener(bool allowAnonymous = false)
        {
            this.allowAnonymous = allowAnonymous;
        }

        public Task<bool> OnAuthenticationRequest(ClientContext context, IAuthenticationEvent authenticationEvent)
        {
            authenticationEvent.Rdn.TryGetValue("cn", out List<string> cnValue);
            authenticationEvent.Rdn.TryGetValue("dc", out List<string> dcValue);

            if ((cnValue == null || cnValue.Count == 0) && string.IsNullOrEmpty(authenticationEvent.Password) && allowAnonymous)
                return Task.FromResult(true);

            if (cnValue.Contains("Manager") && dcValue.Contains("example") && dcValue.Contains("com"))
            {
                return Task.FromResult(true);
            }
            else if (cnValue.Contains("OnlyBindUser") && authenticationEvent.Password == "OnlyBindUserPassword")
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        private string serverRoot = "dc=example,dc=com";

        public Task<SearchResultWrapper> OnSearchRequest(ClientContext context, ISearchEvent searchEvent)
        {
            Console.WriteLine(Convert.ToBase64String(searchEvent.SearchRequest.RawPacket));
            if (context.Rdn.TryGetValue("cn", out var cns) && cns.FirstOrDefault() == "OnlyBindUser")
            {
                return Task.FromResult(new SearchResultWrapper { ResultCode = 32 }); // return 'no such object
            }
            var resultWrapper = new SearchResultWrapper { };
            string limitResultsByPath = null;
            if (searchEvent.SearchRequest.Filter is SearchRequest.PresentFilter presentFilter) // extracting a single item / path
            {
                if (searchEvent.SearchRequest.BaseObject == string.Empty && presentFilter.Value?.ToLower() == "objectclass")
                {
                    resultWrapper.Results = ListAvailableDirectories(searchEvent.SearchRequest.BaseObject);
                    return Task.FromResult(resultWrapper);
                }
                else if (!string.IsNullOrEmpty(searchEvent.SearchRequest.BaseObject))
                {
                    var db = new UserDatabase();
                    var baseObjectPath = searchEvent.SearchRequest.BaseObject?.ToLower();
                    if (baseObjectPath == serverRoot && presentFilter.Value?.ToLower() == "objectclass") // list all subdirectories
                    {
                        resultWrapper.Results = ListAvailableDirectories(searchEvent.SearchRequest.BaseObject);
                        //var allUsers = DumpUsers(db.GetUserDatabase());
                        //resultWrapper.Results.AddRange(allUsers);
                        return Task.FromResult(resultWrapper);
                    }
                    else if (baseObjectPath?.EndsWith(serverRoot) == true && presentFilter.Value?.ToLower() == "objectclass") // list a subdirectory
                    {
                        var directoryDn = baseObjectPath.ValueBefore(serverRoot).ValueAfter("cn=").TrimEnd(',');
                        if (string.IsNullOrEmpty(directoryDn)) // get the root
                        {
                            var directoryUsers = db.GetUserDatabase().Where(u => u.Dn.ToLower().EndsWith(baseObjectPath));
                            var parsedUsers = DumpUsers(directoryUsers);
                            resultWrapper.Results = new List<SearchResultReply>(parsedUsers);
                            return Task.FromResult(resultWrapper);
                        }
                        else if (IsDirectoryFolder(baseObjectPath)) // path is a directory folder
                        {
                            if (searchEvent.SearchRequest.Scope == SearchRequest.ScopeEnum.BaseObject) // get the object
                            {
                                var directory = GenerateDirectory(directoryDn, serverRoot);
                                resultWrapper.Results = new List<SearchResultReply> { directory };
                            }
                            else if (searchEvent.SearchRequest.Scope == SearchRequest.ScopeEnum.SingleLevel) // expand the object
                            {
                                var directoryUsers = db.GetUserDatabase().Where(u => u.Dn.ToLower().EndsWith(baseObjectPath));
                                var parsedUsers = DumpUsers(directoryUsers);
                                resultWrapper.Results = new List<SearchResultReply>(parsedUsers);
                            }
                            return Task.FromResult(resultWrapper);
                        }
                        //if (directoryNames.Contains(directoryDn, StringComparer.OrdinalIgnoreCase)) // list a directory
                        //{
                        //    var directoryUsers = db.GetUserDatabase().Where(u => u.Dn.ToLower().EndsWith(baseObjectPath));
                        //    var parsedUsers = DumpUsers(directoryUsers);
                        //    resultWrapper.Results = new List<SearchResultReply>(parsedUsers);
                        //    return Task.FromResult(resultWrapper);
                        //}
                        //else if (string.IsNullOrEmpty(directoryDn) == true) // list root
                        //{
                        //    var directoryUsers = db.GetUserDatabase().Where(u => u.Dn.ToLower().EndsWith(baseObjectPath));
                        //    var parsedUsers = DumpUsers(directoryUsers);
                        //    resultWrapper.Results = new List<SearchResultReply>(parsedUsers);
                        //    return Task.FromResult(resultWrapper);
                        //}
                        else if (IsObjectInDirectory(baseObjectPath)) // object that is in a directory folder
                        {
                            var allUsers = db.GetUserDatabase().Where(u => u.Dn.ToLower() == searchEvent.SearchRequest.BaseObject?.ToLower()).ToList();
                            var listReply = DumpUsers(allUsers);
                            if (searchEvent.SearchRequest.Scope == SearchRequest.ScopeEnum.BaseObject) // get the object
                            {
                                resultWrapper.Results = listReply;
                            }
                            //if (searchEvent.SearchRequest.AttributeSelection.All(x => x.ToLower() != "objectclass")) // just check if it's available
                            //    resultWrapper.Results = listReply;
                            return Task.FromResult(resultWrapper); // libs makes an object not found out of this.. it should return an OK since
                        }
                        else // unknown path
                        {
                            return Task.FromResult(new SearchResultWrapper { ResultCode = 32 }); // return 'no such object
                        }
                    }
                    else // get single item
                    {
                        var allUsers = db.GetUserDatabase().Where(u => u.Dn.ToLower() == searchEvent.SearchRequest.BaseObject?.ToLower()).ToList();
                        var listReply = DumpUsers(allUsers);
                        if (searchEvent.SearchRequest.AttributeSelection.All(x => x.ToLower() != "objectclass")) // just check if it's available
                            resultWrapper.Results = listReply;
                        return Task.FromResult(resultWrapper); // libs makes an object not found out of this.. it should return an OK since
                    }
                }
            }
            else // it's a search
            {
                var baseObjectPath = searchEvent.SearchRequest.BaseObject?.ToLower();
                //var directoryDn = baseObjectPath.ValueBefore(serverRoot).ValueAfter("cn=").TrimEnd(',');
                //if (string.IsNullOrEmpty(directoryDn)) // search on the root
                //{
                //    var directoryUsers = db.GetUserDatabase().Where(u => u.Dn.ToLower().EndsWith(baseObjectPath));
                //    var parsedUsers = DumpUsers(directoryUsers);
                //    resultWrapper.Results = new List<SearchResultReply>(parsedUsers);
                //    return Task.FromResult(resultWrapper);
                //}
                if (IsDirectoryFolder(baseObjectPath)) // path is a directory folder
                {
                    limitResultsByPath = baseObjectPath;
                }
            }

            RestoreLdapFilter(searchEvent.SearchRequest.Filter);

            int? limit = searchEvent.SearchRequest.SizeLimit;

            // Load the user database that queries will be executed against
            UserDatabase dbContainer = new UserDatabase();
            IQueryable<UserDatabase.User> userDb = dbContainer.GetUserDatabase().AsQueryable();

            var itemExpression = Expression.Parameter(typeof(UserDatabase.User));
            SearchExpressionBuilder searchExpressionBuilder = new SearchExpressionBuilder(searchEvent);
            var conditions = searchExpressionBuilder.Build(searchEvent.SearchRequest.Filter, itemExpression);
            var queryLambda = Expression.Lambda<Func<UserDatabase.User, bool>>(conditions, itemExpression);
            var predicate = queryLambda.Compile();

            var results = userDb.Where(predicate).ToList();
            if (!string.IsNullOrEmpty(limitResultsByPath))
                results = results.Where(u => u.Dn.ToLower().EndsWith(limitResultsByPath)).ToList();

            List<SearchResultReply> replies = new List<SearchResultReply>();
            foreach (UserDatabase.User user in results)
            {
                List<SearchResultReply.Attribute> attributes = new List<SearchResultReply.Attribute>();
                SearchResultReply reply = new SearchResultReply(
                    user.Dn,
                    attributes
                );

                foreach (KeyValuePair<string, List<string>> attribute in user.Attributes)
                {
                    SearchResultReply.Attribute attributeClass = new SearchResultReply.Attribute(attribute.Key, attribute.Value);
                    attributes.Add(attributeClass);
                }

                replies.Add(reply);
            }

            resultWrapper.Results = replies;
            return Task.FromResult(resultWrapper);
        }

        private bool IsObjectInDirectory(string path)
        {
            var directoryPaths = directoryNames.Select(x => GetDirectoryPath(x)).ToList();
            return directoryPaths.Any(x => path.EndsWith($",{x.ToLower()}") == true);
        }

        /// <summary>
        /// checks if the object is a directory folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsDirectoryFolder(string path)
        {
            var directoryPaths = directoryNames.Select(x => GetDirectoryPath(x)).ToList();
            return directoryPaths.Any(x => string.Compare(x, path, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private void RestoreLdapFilter(SearchRequest.IFilterChoice filter)
        {
            var container = new SimpleParsedFilter();
            RestoreLdapFilter(filter, container);
            var parsedFilter = GetSearchFilter(container);
            Console.WriteLine($"Parsed filter {filter}: {parsedFilter}");
        }

        private List<string> blockedAttributes = new List<string> { "objectclass", "objectcategory" };

        private string GetSearchFilter(SimpleParsedFilter container)
        {
            var substringFilters = container.AttributeFilters.Where(u => u.Operator == Operator.Substring).ToList();
            var searchTerms = container.AttributeFilters
                .Where(u => u.Operator == Operator.Substring || (u.Operator == Operator.Equals && !blockedAttributes.Contains(u.Attribute.ToLower())))
                .Select(x => x.AttributeValue)
                .Distinct()
                .ToList();
            return string.Join(' ', searchTerms);
        }

        private void RestoreLdapFilter(SearchRequest.IFilterChoice filter, SimpleParsedFilter container)
        {
            if (filter is SearchRequest.AndFilter andFilter)
            {
                foreach (var subFilter in andFilter.Filters)
                {
                    RestoreLdapFilter(subFilter, container);
                }
            }
            else if (filter is SearchRequest.OrFilter orFilter)
            {
                foreach (var subFilter in orFilter.Filters)
                {
                    RestoreLdapFilter(subFilter, container);
                }
            }
            else if (filter is SearchRequest.NotFilter notFilter)
            {
                RestoreLdapFilter(notFilter.Filter, container);
            }
            else if (filter is SearchRequest.EqualityMatchFilter equalFilter)
            {
                container.AttributeFilters.Add(new AttributeFilter
                {
                    Attribute = equalFilter.AttributeDesc,
                    AttributeValue = equalFilter.AssertionValue,
                    Operator = Operator.Equals
                });
            }
            else if (filter is SearchRequest.GreaterOrEqualFilter gtFilter)
            {
                container.AttributeFilters.Add(new AttributeFilter
                {
                    Attribute = gtFilter.AttributeDesc,
                    AttributeValue = gtFilter.AssertionValue,
                    Operator = Operator.GreaterOrEqual
                });
            }
            else if (filter is SearchRequest.LessOrEqualFilter ltFilter)
            {
                container.AttributeFilters.Add(new AttributeFilter
                {
                    Attribute = ltFilter.AttributeDesc,
                    AttributeValue = ltFilter.AssertionValue,
                    Operator = Operator.LessOrEqual
                });
            }
            else if (filter is SearchRequest.SubstringFilter subFilter)
            {
                container.AttributeFilters.Add(new AttributeFilter
                {
                    Attribute = subFilter.AttributeDesc,
                    AttributeValue = subFilter.Initial ?? string.Join("*", subFilter.Any),
                    Operator = Operator.Substring
                });
            }
        }

        private List<SearchResultReply> DumpUsers(IEnumerable<UserDatabase.User> users)
        {
            var replies = new List<SearchResultReply>();
            foreach (var user in users)
            {
                var attributes = new List<SearchResultReply.Attribute>();
                SearchResultReply reply = new SearchResultReply(
                    user.Dn,
                    attributes
                );
                foreach (var attribute in user.Attributes)
                {
                    var attributeClass = new SearchResultReply.Attribute(attribute.Key, attribute.Value);
                    attributes.Add(attributeClass);
                }
                replies.Add(reply);
            }
            return replies;
        }

        //private List<SearchResultReply> ListDirectories(string path)
        //{
        //    var result = new List<SearchResultReply>();
        //    var attributes = new List<SearchResultReply.Attribute>();

        //    if (string.IsNullOrEmpty(path)) // root
        //    {
        //        var cn = "example";
        //        var rootPath = "com";

        //        var dn = $"CN={cn},DC={rootPath}";

        //        attributes.Add(new SearchResultReply.Attribute("cn", new List<string> { cn }));
        //        attributes.Add(new SearchResultReply.Attribute("distinguishedName", new List<string> { dn }));
        //        attributes.Add(new SearchResultReply.Attribute("objectClass", new List<string> { "top", "container" }));
        //        //attributes.Add(new SearchResultReply.Attribute("instanceType", new List<string> { "4" }));
        //        //attributes.Add(new SearchResultReply.Attribute("objectCategory", new List<string> { "CN=Container,CN=Schema,CN=Configuration,CN={2CFB8545-C0CC-4513-B6D3-600A7357D5E2}" }));

        //        var reply = new SearchResultReply(dn, attributes);
        //        result.Add(reply);
        //    }

        //    return result;
        //}

        private List<SearchResultReply> ListAvailableDirectories(string path)
        {
            var result = new List<SearchResultReply>();
            var attributes = new List<SearchResultReply.Attribute>();

            if (string.IsNullOrEmpty(path)) // root
            {
                attributes.Add(new SearchResultReply.Attribute("namingContexts", new List<string> { serverRoot }));

                //var cn = "example";
                //var rootPath = "com";

                //var dn = $"DC={cn},DC={rootPath}";

                //attributes.Add(new SearchResultReply.Attribute("cn", new List<string> { cn }));
                //attributes.Add(new SearchResultReply.Attribute("distinguishedName", new List<string> { dn }));
                //attributes.Add(new SearchResultReply.Attribute("objectClass", new List<string> { "top", "container" }));
                ////attributes.Add(new SearchResultReply.Attribute("instanceType", new List<string> { "4" }));
                ////attributes.Add(new SearchResultReply.Attribute("objectCategory", new List<string> { "CN=Container,CN=Schema,CN=Configuration,CN={2CFB8545-C0CC-4513-B6D3-600A7357D5E2}" }));

                var reply = new SearchResultReply(string.Empty, attributes);
                result.Add(reply);
            }

            else
            {
                foreach (var dirName in directoryNames)
                {
                    result.Add(GenerateDirectory(dirName, path));
                }
            }

            return result;
        }

        private readonly List<string> directoryNames = new List<string> { "ActiveDirectory", "Zuweiser", "ETV.online" };

        private string GetDirectoryPath(string directoryName, string path = null)
        {
            return $"CN={directoryName},{path ?? serverRoot}";
        }

        private SearchResultReply GenerateDirectory(string name, string path)
        {
            var attributes = new List<SearchResultReply.Attribute>();

            var cn = $"{name}";
            var dn = GetDirectoryPath(name, path);

            attributes.Add(new SearchResultReply.Attribute("cn", new List<string> { cn }));
            attributes.Add(new SearchResultReply.Attribute("distinguishedName", new List<string> { dn }));
            attributes.Add(new SearchResultReply.Attribute("objectClass", new List<string> { "top", "container" }));
            attributes.Add(new SearchResultReply.Attribute("instanceType", new List<string> { "4" }));
            attributes.Add(new SearchResultReply.Attribute("objectCategory", new List<string> { "CN=Container,CN=Schema,CN=Configuration,CN={2CFB8545-C0CC-4513-B6D3-600A7357D5E2}" }));

            var reply = new SearchResultReply(dn, attributes);
            return reply;
        }
    }
}
