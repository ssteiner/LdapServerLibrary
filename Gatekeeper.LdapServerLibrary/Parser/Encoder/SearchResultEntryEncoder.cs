using Gatekeeper.LdapServerLibrary.Models.Operations.Response;
using System.Formats.Asn1;

namespace Gatekeeper.LdapServerLibrary.Parser.Encoder
{
    internal class SearchResultEntryEncoder : IApplicationEncoder<SearchResultEntry>
    {
        public AsnWriter TryEncode(AsnWriter writer, SearchResultEntry message)
        {
            Asn1Tag searchResultEntryApplication = new Asn1Tag(TagClass.Application, 4);

            using (writer.PushSequence(searchResultEntryApplication))
            {
                writer.WriteOctetString(System.Text.Encoding.ASCII.GetBytes(message.SearchResultReply.CommonName));
                using (writer.PushSequence())
                {
                    foreach (var attribute in message.SearchResultReply.Attributes)
                    {
                        using (writer.PushSequence())
                        {
                            writer.WriteOctetString(System.Text.Encoding.ASCII.GetBytes(attribute.Key));
                            using (writer.PushSetOf())
                            {
                                if (attribute.Values != null)
                                {
                                    if (attribute.IsOid)
                                    {
                                        foreach (string value in attribute.Values)
                                        {
                                            if (value != null)
                                                writer.WriteObjectIdentifier(value);
                                            else
                                                writer.WriteNull();
                                        }
                                    }
                                    else
                                    {
                                        foreach (string value in attribute.Values)
                                        {
                                            if (value != null)
                                                writer.WriteOctetString(System.Text.Encoding.UTF8.GetBytes(value));
                                            else
                                                writer.WriteNull();
                                        }
                                    }
                                }
                                if (attribute.ByteValue != null)
                                {
                                    if (attribute.ByteValue != null)
                                        writer.WriteOctetString(attribute.ByteValue);
                                    else
                                        writer.WriteNull();
                                }
                            }
                        }
                    }
                }
            }
            return writer;
        }
    }
}