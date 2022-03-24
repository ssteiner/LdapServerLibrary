using System.Net.Sockets;
using System.Threading;

namespace Gatekeeper.LdapServerLibrary.Network
{
    internal class ConnectionManager
    {
        internal void AddClient(TcpClient client, CancellationToken cancellationToken)
        {
            ClientSession session = new ClientSession(client, cancellationToken);
            session.StartReceiving();
        }
    }
}
