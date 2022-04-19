namespace Gatekeeper.LdapServerLibrary.Models.Operations.Response
{
    internal class AbandonResponse: IProtocolOp
    {
        int IProtocolOp.GetTag()
        {
            return -1;
        }
    }
}
