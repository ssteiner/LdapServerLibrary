using System;
using System.Collections.Generic;

namespace Sample
{
    internal class UserDatabase
    {

        private readonly static string BasePath = @"C:\Temp\pictures\";
        private static readonly List<string> objectclasses = new List<string> { "top", "person", "organizationalPerson" };

        private static byte[] ReadPicture(string path)
        {
            var fullPath = $"{BasePath}{path}";
            if (System.IO.File.Exists(fullPath))
                return System.IO.File.ReadAllBytes(fullPath);
            return null;
        }

        private static string GetPicture(string path)
        {
            var content = ReadPicture(path);
            if (content != null)
                return System.Text.Encoding.ASCII.GetString(content);
            return null;
        }

        private static string ConvertGuid(string guidString)
        {
            if (Guid.TryParse(guidString, out var myGuid))
                return ToObjectGUIDSearchString(myGuid);
            return guidString;
        }

        private static string ToObjectGUIDSearchString(Guid ObjectGUID)
        {
            var byteGuid = ObjectGUID.ToByteArray();
            var queryGuid = "";
            foreach (var b in byteGuid)
            {
                queryGuid += $@"\{b:x2}";
            }
            return queryGuid;
        }

        private static byte[] ConvertGuidToArray(string guidString)
        {
            if (Guid.TryParse(guidString, out var myGuid))
                return myGuid.ToByteArray();
            return null;
        }

        private static byte[] ConvertSid(string sidString)
        {
            var identifier = new System.Security.Principal.SecurityIdentifier(sidString);
            var byteArray = new byte[identifier.BinaryLength];
            identifier.GetBinaryForm(byteArray, 0);
            return byteArray;

            //new System.Security.Principal.securit

            //var charArray = System.Text.Encoding.ASCII.GetBytes(sidString);
            //return charArray;
        }

        private readonly List<User> Users = new List<User>{
            new User{
                Dn = "cn=test1,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"test1@example.com"}},
                    {"role", new List<string>(){"Administrator"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Test User 1"}},
                    {"uid", new List<string>() {"test1"}},
                    {"givenname", new List<string>() {"User 1"}},
                    {"sn", new List<string>() {"Test"}},
                    {"telephonenumber", new List<string>() {"+41587771000"}},
                },
            },
            new User{
                Dn = "cn=test2,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"test2@example.com", "test2-alias@example.com"}},
                    {"role", new List<string>(){"Employee"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Test User 2"}},
                    {"uid", new List<string>() {"test2"}},
                    {"givenname", new List<string>() {"User 2"}},
                    {"sn", new List<string>() {"Test"}},
                    {"telephonenumber", new List<string>() {"+41587771001"}},
                },
            },
            new User{
                Dn = "cn=test3,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"test3@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Test User 3"}},
                    {"uid", new List<string>() {"test3"}},
                    {"givenname", new List<string>() {"User 3"}},
                    {"sn", new List<string>() {"Test"}},
                    {"telephonenumber", new List<string>() {"+41587771002"}},
                },
            },
            new User{
                Dn = "cn=benutzer4,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"benutzer4@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Benutzer 4"}},
                    {"uid", new List<string>() {"test4"}},
                    {"givenname", new List<string>() {"User 4"}},
                    {"sn", new List<string>() {"Test"}},
                    {"telephonenumber", new List<string>() {"+41587771003"}},
                },
            },
            new User{
                Dn = "cn=adUser1,cn=ActiveDirectory,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    {"email", new List<string>(){"adUser1@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"AD Benützer 1"}},
                    {"uid", new List<string>() {"adTest1"}},
                    {"givenname", new List<string>() {"Benützer 1"}},
                    {"sn", new List<string>() {"AD"}},
                    {"telephonenumber", new List<string>() {"+41587771004"}},
                },
                ByteAttributes = new Dictionary<string, byte[]>
                {
                    {"objectGUID", ConvertGuidToArray("{D68F41B8-0383-444E-9A67-A75FA9DB6C11}") },
                    {"objectSid", ConvertSid("S-1-5-21-2893744555-524998179-1716630349-830211") },
                    {"thumbnailphoto", ReadPicture("ciscotest1@nxodev.intra.jpg") }
                }
            },
            new User{
                Dn = "cn=adUser2,cn=ActiveDirectory,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    {"email", new List<string>(){"adUser2@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"AD Benützer 2"}},
                    {"uid", new List<string>() {"adTest2"}},
                    {"givenname", new List<string>() {"Benützer 2"}},
                    {"sn", new List<string>() {"AD"}},
                    {"telephonenumber", new List<string>() {"+41587771005"}},
                },
                ByteAttributes = new Dictionary<string, byte[]>
                {
                    {"thumbnailphoto", ReadPicture("ciscotest2@nxodev.intra.jpg") }
                }
            },
            new User{
                Dn = "cn=adUser3,cn=ActiveDirectory,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    {"email", new List<string>(){"adUser3@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"AD Benutzer 3"}},
                    {"uid", new List<string>() {"adTest3"}},
                    {"givenname", new List<string>() {"Benutzer 2"}},
                    {"sn", new List<string>() {"AD"}},
                    {"telephonenumber", new List<string>() {"+41587771006"}},
                },
                ByteAttributes = new Dictionary<string, byte[]>
                {
                    {"thumbnailphoto", ReadPicture("ciscotest3@nxodev.intra.jpg") }
                }
            },
            new User{
                Dn = "cn=sste,cn=ActiveDirectory,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    {"email", new List<string>(){"sste@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Stephan Steiner"}},
                    {"uid", new List<string>() {"sste"}},
                    {"givenname", new List<string>() {"Stephan"}},
                    {"sn", new List<string>() {"Steiner"}},
                    {"telephonenumber", new List<string>() {"+41587771206"}},
                },
                ByteAttributes = new Dictionary<string, byte[]>
                {
                    {"thumbnailphoto", ReadPicture("ciscotest5@nxodev.intra.jpg") }
                }
            },
            new User{
                Dn = "cn=ciscotest1,cn=ActiveDirectory,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    {"email", new List<string>(){"ciscotest1@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Cisco Test 1"}},
                    {"uid", new List<string>() {"ciscotest1"}},
                    {"givenname", new List<string>() {"Test 1"}},
                    {"sn", new List<string>() {"Cisco"}},
                    {"telephonenumber", new List<string>() {"+41587771207"}},
                },
                ByteAttributes = new Dictionary<string, byte[]>
                {
                    {"thumbnailphoto", ReadPicture("ciscotest5@nxodev.intra.jpg") }
                }
            },
            new User{
                Dn = "cn=alcateltest1,cn=ActiveDirectory,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    {"email", new List<string>(){"alcateltest1@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Alcatel Test 1"}},
                    {"uid", new List<string>() {"alcateltest1"}},
                    {"givenname", new List<string>() {"Test 1"}},
                    {"sn", new List<string>() {"Alcatel"}},
                    {"telephonenumber", new List<string>() {"+41587771208"}},
                },
                ByteAttributes = new Dictionary<string, byte[]>
                {
                    {"thumbnailphoto", ReadPicture("ciscotest5@nxodev.intra.jpg") }
                }
            },
            new User{
                Dn = "cn=mstest1,cn=ActiveDirectory,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    {"email", new List<string>(){"mstest1@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Alcatel Test 1"}},
                    {"uid", new List<string>() {"mstest1"}},
                    {"givenname", new List<string>() {"Test 1"}},
                    {"sn", new List<string>() {"Microsoft"}},
                    {"telephonenumber", new List<string>() {"+41587771209"}},
                },
                ByteAttributes = new Dictionary<string, byte[]>
                {
                    {"thumbnailphoto", ReadPicture("ciscotest5@nxodev.intra.jpg") }
                }
            },
            new User{
                Dn = "cn=zuser1,cn=Zuweiser,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"zuser1@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Zuweiser 14"}},
                    {"uid", new List<string>() {"user1_zuweiser"}},
                    {"givenname", new List<string>() {"Zuweiser"}},
                    {"sn", new List<string>() {"Test 1"}},
                    {"telephonenumber", new List<string>() {"+41587771007"}},
                },
                ByteAttributes = new Dictionary<string, byte[]>
                {
                    {"thumbnailphoto", ReadPicture("lsste@nxodev.intra.jpg") }
                }
            },
            new User{
                Dn = "cn=zuser2,cn=Zuweiser,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"zuser2@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Zuweiser 2"}},
                    {"uid", new List<string>() {"user2_zuweiser"}},
                    {"givenname", new List<string>() {"Zuweiser"}},
                    {"sn", new List<string>() {"Test 2"}},
                    {"telephonenumber", new List<string>() {"+41587771008"}},
                },
            },
            new User{
                Dn = "cn=etvuser1,cn=ETV.online,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"dumitru.meister@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"ETV Online One"}},
                    {"sn", new List<string>() {"Meister"}},
                    {"givenname", new List<string>() {"Dumitru"}},
                    {"uid", new List<string>() {"etv_user1"}},
                    {"telephonenumber", new List<string>() {"+41587771009"}},
                },
            },
            new User{
                Dn = "cn=etvuser2,cn=ETV.online,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"thomas.freiburghaus@example.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Thomas Freiburghaus"}},
                    {"sn", new List<string>() {"Freiburghaus"}},
                    {"givenname", new List<string>() {"Thomas"}},
                    {"uid", new List<string>() {"etv_user2"}},
                    {"telephonenumber", new List<string>() {"+41587771010"}},
                },
            },
            new User{
                Dn = "cn=etvuser3,cn=ETV.online,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"cornelia.bruegger@etvonline.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Cornelia Brügger"}},
                    {"sn", new List<string>() {"Brügger"}},
                    {"givenname", new List<string>() {"Cornelia"}},
                    {"uid", new List<string>() {"etv_user3"}},
                    {"telephonenumber", new List<string>() {"+41587771011"}},
                },
            },
            new User{
                Dn = "cn=etvuser4,cn=ETV.online,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"beat.kuster@etvonline.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Beat Kuster"}},
                    {"sn", new List<string>() {"Kuster"}},
                    {"givenname", new List<string>() {"Beat"}},
                    {"uid", new List<string>() {"etv_user4"}},
                    {"telephonenumber", new List<string>() {"+41587771012"}},
                },
            },
            new User{
                Dn = "cn=etvuser5,cn=ETV.online,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase){
                    {"email", new List<string>(){"hans.meister@etvonline.com"}},
                    {"objectclass", objectclasses},
                    {"objectcategory", new List<string>() {"person"}},
                    {"displayname", new List<string>() {"Hans Meister"}},
                    {"sn", new List<string>() {"Meister"}},
                    {"givenname", new List<string>() {"Hans"}},
                    {"uid", new List<string>() {"etv_user5"}},
                    {"telephonenumber", new List<string>() {"+41587771013"}},
                },
            },
        };

        internal List<User> GetUserDatabase()
        {
            return Users;
        }

        internal class User
        {
            internal string Dn { get; set; }
            internal Dictionary<string, List<string>> Attributes { get; set; }

            internal Dictionary<string, byte[]> ByteAttributes { get; set; }
        }
    }
}
