using System;
using System.Collections.Generic;

namespace Sample
{
    internal class UserDatabase
    {

        private readonly static string BasePath = @"C:\Temp\pictures\";

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
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"test1@example.com"}},
                    {"role", new List<string>(){"Administrator"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"Test User 1"}},
                    {"uid", new List<string>() {"test1"}},
                },
            },
            new User{
                Dn = "cn=test2,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"test2@example.com", "test2-alias@example.com"}},
                    {"role", new List<string>(){"Employee"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"Test User 2"}},
                    {"uid", new List<string>() {"test2"}},
                },
            },
            new User{
                Dn = "cn=test3,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"test3@example.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"Test User 3"}},
                    {"uid", new List<string>() {"test3"}},
                },
            },
            new User{
                Dn = "cn=benutzer4,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"benutzer4@example.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"Benutzer 4"}},
                    {"uid", new List<string>() {"test4"}},
                },
            },
            new User{
                Dn = "cn=adUser1,cn=ActiveDirectory,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>()
                {
                    {"email", new List<string>(){"adUser1@example.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"AD Benützer 1"}},
                    {"uid", new List<string>() {"adTest1"}},
                    //{"thumbnailphoto", new List<string>{ GetPicture("ciscotest1@nxodev.intra.jpg") } },
                    //{"objectGUID", new List<string>() { ConvertGuid("{D68F41B8-0383-444E-9A67-A75FA9DB6C11}") }},
                    //{"objectSid", new List<string>() {"S-1-5-21-2893744555-524998179-1716630349-830211"}},
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
                Attributes = new Dictionary<string, List<string>>()
                {
                    {"email", new List<string>(){"adUser2@example.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"AD Benützer 2"}},
                    {"uid", new List<string>() {"adTest2"}},
                    {"thumbnailphoto", new List<string>{ GetPicture("ciscotest2@nxodev.intra.jpg") } }
                },
            },
            new User{
                Dn = "cn=adUser3,cn=ActiveDirectory,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>()
                {
                    {"email", new List<string>(){"adUser3@example.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"AD Benutzer 3"}},
                    {"uid", new List<string>() {"adTest3"}},
                    {"thumbnailphoto", new List<string>{ GetPicture("ciscotest3@nxodev.intra.jpg") } }
                },
            },
            new User{
                Dn = "cn=zuser1,cn=Zuweiser,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"zuser1@example.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"Zuweiser 14"}},
                    {"uid", new List<string>() {"user1_zuweiser"}},
                    {"thumbnailphoto", new List<string>{ GetPicture("lsste@nxodev.intra.jpg") } }
                },
            },
            new User{
                Dn = "cn=zuser2,cn=Zuweiser,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"zuser2@example.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"Zuweiser 2"}},
                    {"uid", new List<string>() {"user2_zuweiser"}},
                },
            },
            new User{
                Dn = "cn=etvuser1,cn=ETV.online,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"dumitru.meister@example.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"ETV Online One"}},
                    {"sn", new List<string>() {"Meister"}},
                    {"givenName", new List<string>() {"Dumitru"}},
                    {"uid", new List<string>() {"etv_user1"}},
                },
            },
            new User{
                Dn = "cn=etvuser2,cn=ETV.online,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"thomas.freiburghaus@example.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"Thomas Freiburghaus"}},
                    {"sn", new List<string>() {"Freiburghaus"}},
                    {"givenName", new List<string>() {"Thomas"}},
                    {"uid", new List<string>() {"etv_user2"}},
                },
            },
            new User{
                Dn = "cn=etvuser3,cn=ETV.online,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"cornelia.bruegger@etvonline.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"Cornelia Brügger"}},
                    {"sn", new List<string>() {"Brügger"}},
                    {"givenName", new List<string>() {"Cornelia"}},
                    {"uid", new List<string>() {"etv_user3"}},
                },
            },
            new User{
                Dn = "cn=etvuser4,cn=ETV.online,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"beat.kuster@etvonline.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"Beat Kuster"}},
                    {"sn", new List<string>() {"Kuster"}},
                    {"givenName", new List<string>() {"Beat"}},
                    {"uid", new List<string>() {"etv_user4"}},
                },
            },
            new User{
                Dn = "cn=etvuser5,cn=ETV.online,dc=example,dc=com",
                Attributes = new Dictionary<string, List<string>>(){
                    {"email", new List<string>(){"hans.meister@etvonline.com"}},
                    {"objectclass", new List<string>(){"inetOrgPerson"}},
                    {"displayname", new List<string>() {"Hans Meister"}},
                    {"sn", new List<string>() {"Meister"}},
                    {"givenName", new List<string>() {"Hans"}},
                    {"uid", new List<string>() {"etv_user5"}},
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
