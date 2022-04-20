using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;
using Gatekeeper.LdapServerLibrary.Session.Replies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample
{
    internal static class LdapUtil
    {

        internal static string ObjectClass = "objectclass";
        internal static string NamingContexts = "namingContexts";
        internal static string SubSchemaSubEntry = "subschemaSubEntry";

        private static readonly List<string> blockedAttributes = new() { ObjectClass, "objectcategory" };

        internal static ParsedLdapQuery RestoreLdapFilter(SearchRequest.IFilterChoice filter)
        {
            var container = new SimpleParsedFilter();
            var ldapFilter = RestoreLdapFilter(filter, container);
            var query = new ParsedLdapQuery { LdapQuery = ldapFilter };
            GetSearchFilter(container, query);
            return query;
        }

        private static void GetSearchFilter(SimpleParsedFilter container, ParsedLdapQuery query)
        {
            var substringFilters = container.AttributeFilters.Where(u => u.Operator == Operator.Substring).ToList();
            var searchTerms = container.AttributeFilters
                .Where(u => u.Operator == Operator.Substring || u.Operator == Operator.GreaterOrEqual || u.Operator == Operator.LessOrEqual 
                    || (u.Operator == Operator.Equals && !blockedAttributes.Contains(u.Attribute.ToLower())))
                .Select(x => new { x.Attribute, x.AttributeValue })
                .Distinct()
                .ToList();
            query.SimpleQuery = string.Join(' ', searchTerms.Select(x => x.AttributeValue).Distinct());
            query.SearchedAttributes = searchTerms.Select(x => x.Attribute).ToList();
        }

        private static string RestoreLdapFilter(SearchRequest.IFilterChoice filter, SimpleParsedFilter container)
        {
            if (filter is SearchRequest.AndFilter andFilter)
            {
                var tempFilter = $"(&";
                foreach (var subFilter in andFilter.Filters)
                {
                    tempFilter += $"{RestoreLdapFilter(subFilter, container)}";
                }
                tempFilter = $"{tempFilter})";
                return tempFilter;
            }
            else if (filter is SearchRequest.OrFilter orFilter)
            {
                var tempFilter = $"(|";
                foreach (var subFilter in orFilter.Filters)
                {
                    tempFilter += $"{RestoreLdapFilter(subFilter, container)}";
                }
                tempFilter = $"{tempFilter})";
                return tempFilter;
            }
            else if (filter is SearchRequest.NotFilter notFilter)
            {
                return $"(!({RestoreLdapFilter(notFilter.Filter, container)}))";
            }
            else if (filter is SearchRequest.EqualityMatchFilter equalFilter)
            {
                container.AttributeFilters.Add(new AttributeFilter
                {
                    Attribute = equalFilter.AttributeDesc,
                    AttributeValue = equalFilter.AssertionValue,
                    Operator = Operator.Equals
                });
                return $"({equalFilter.AttributeDesc}={equalFilter.AssertionValue})";
            }
            else if (filter is SearchRequest.GreaterOrEqualFilter gtFilter)
            {
                container.AttributeFilters.Add(new AttributeFilter
                {
                    Attribute = gtFilter.AttributeDesc,
                    AttributeValue = gtFilter.AssertionValue,
                    Operator = Operator.GreaterOrEqual
                });
                return $"({gtFilter.AttributeDesc}>={gtFilter.AssertionValue})";
            }
            else if (filter is SearchRequest.LessOrEqualFilter ltFilter)
            {
                container.AttributeFilters.Add(new AttributeFilter
                {
                    Attribute = ltFilter.AttributeDesc,
                    AttributeValue = ltFilter.AssertionValue,
                    Operator = Operator.LessOrEqual
                });
                return $"({ltFilter.AttributeDesc}<={ltFilter.AssertionValue})";
            }
            else if (filter is SearchRequest.SubstringFilter subFilter)
            {
                container.AttributeFilters.Add(new AttributeFilter
                {
                    Attribute = subFilter.AttributeDesc,
                    AttributeValue = subFilter.Initial ?? subFilter.Final ?? $"{string.Join(string.Empty, subFilter.Any.Select(x => $"*{x}"))}*",
                    Operator = Operator.Substring
                });
                if (!string.IsNullOrEmpty(subFilter.Initial))
                    return $"({subFilter.AttributeDesc}={subFilter.Initial}*)";
                else if (!string.IsNullOrEmpty(subFilter.Final))
                    return $"({subFilter.AttributeDesc}=*{subFilter.Initial})";
                else
                    return $"({subFilter.AttributeDesc}={string.Join(string.Empty, subFilter.Any.Select(x => $"*{x}"))}*)";
            }
            return null;
        }

        internal static bool IsDirectoryFolder(string path, List<string> directoryNames, string serverRoot)
        {
            var directoryPaths = directoryNames.Select(x => GetDirectoryPath(x, serverRoot)).ToList();
            return directoryPaths.Any(x => string.Compare(x, path, StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal static bool IsObjectInDirectory(string path, List<string> directoryNames, string serverRoot, bool includeRoot = false)
        {
            var directoryPaths = directoryNames.Select(x => GetDirectoryPath(x, serverRoot)).ToList();
            if (includeRoot)
                directoryPaths.Add(serverRoot);
            return directoryPaths.Any(x => path.EndsWith($",{x.ToLower()}") == true);
        }

        internal static string GetDirectoryPath(string directoryName, string path)
        {
            return $"CN={directoryName},{path}";
        }

        internal static SearchResultReply GenerateDirectory(string name, string path, List<string> attributeList)
        {
            var attributes = new List<SearchResultReply.Attribute>();
            var cn = $"{name}";
            var dn = GetDirectoryPath(name, path);

            if (attributeList.Any() && attributeList.All(x => x == ObjectClass)) // only asking for object class
                attributes.Add(new SearchResultReply.Attribute("objectClass", new List<string> { "top", "container" }));
            else // to be proper, we should only return all attributes if they're requested or if none are requested
            {
                attributes.Add(new SearchResultReply.Attribute("cn", new List<string> { cn }));
                attributes.Add(new SearchResultReply.Attribute("name", new List<string> { cn }));
                attributes.Add(new SearchResultReply.Attribute("distinguishedName", new List<string> { dn }));
                attributes.Add(new SearchResultReply.Attribute("objectClass", new List<string> { "top", "container" }));
                attributes.Add(new SearchResultReply.Attribute("instanceType", new List<string> { "4" }));
                attributes.Add(new SearchResultReply.Attribute("objectCategory", new List<string> { "CN=Container,CN=Schema,CN=Configuration,CN={2CFB8545-C0CC-4513-B6D3-600A7357D5E2}" }));
            }
            var reply = new SearchResultReply(dn, attributes);
            return reply;
        }

        internal static List<SearchResultReply> ListAvailableDirectories(string path, string serverRoot, List<string> directoryNames, List<string> attibuteList)
        {
            var result = new List<SearchResultReply>();
            if (string.IsNullOrEmpty(path)) // root
            {
                result.Add(GetRoot(serverRoot, attibuteList));
            }
            else
            {
                foreach (var dirName in directoryNames)
                {
                    result.Add(GenerateDirectory(dirName, path, attibuteList));
                }
            }
            return result;
        }

        internal static SearchResultReply GetRoot(string serverRoot, List<string> attributeList)
        {
            var attributes = new List<SearchResultReply.Attribute>();
            if (attributeList.Contains(NamingContexts, StringComparer.OrdinalIgnoreCase))
            {
                attributes.Add(new SearchResultReply.Attribute(NamingContexts, new List<string> { serverRoot }));
                if (attributeList.All(x => x == NamingContexts)) // only querying naming contexts
                    return new SearchResultReply(string.Empty, attributes); 
            }
            if (attributeList.Count == 0 || attributeList.Contains(ObjectClass, StringComparer.OrdinalIgnoreCase))
                attributes.Add(new SearchResultReply.Attribute(ObjectClass, new List<string> { "top", "container" }));
            if (attributeList.Contains("supportedCapabilities"))
            { // no additional capabilities
                attributes.Add(new SearchResultReply.Attribute("supportedCapabilities", new List<string> { }));
            }
            if (attributeList.Contains("supportedLDAPVersion"))
                attributes.Add(new SearchResultReply.Attribute("supportedLDAPVersion", new List<string> { "2", "3" }));
            if (attributeList.Contains("supportedLDAPPolicies"))
                attributes.Add(new SearchResultReply.Attribute("supportedLDAPPolicies", new List<string> { }));
            //if (attributeList.Contains("dnsHostName")) // hostname of DNS server?
            //    attributes.Add(new SearchResultReply.Attribute("supportedLDAPPolicies", new List<string> { }));
            if (attributeList.Contains("supportedSASLMechanisms"))
                attributes.Add(new SearchResultReply.Attribute("supportedSASLMechanisms", new List<string> { }));

            if (attributeList.Contains("defaultNamingContext", StringComparer.OrdinalIgnoreCase))
                attributes.Add(new SearchResultReply.Attribute("defaultNamingContext", new List<string> { serverRoot }));

            if (attributeList.Contains("supportedControl", StringComparer.OrdinalIgnoreCase))
                attributes.Add(new SearchResultReply.Attribute("supportedControls", new List<string> { "1.2.840.113556.1.4.474", "1.2.840.113556.1.4.473" }, true));

            if (attributeList.Count == 0 || attributeList.Contains("distinguishedName", StringComparer.OrdinalIgnoreCase))
                attributes.Add(new SearchResultReply.Attribute("distinguishedName", new List<string> { serverRoot }));
            if (attributeList.Count == 0 || attributeList.Contains("name", StringComparer.OrdinalIgnoreCase))
                attributes.Add(new SearchResultReply.Attribute("name", new List<string> { "root" }));
            return new SearchResultReply(string.Empty, attributes);
            //add other stuff
        }

    }

    internal class ParsedLdapQuery
    {
        public string LdapQuery { get; set; }

        public string SimpleQuery { get; set; }

        public List<string> SearchedAttributes { get; set; }
    }


}
