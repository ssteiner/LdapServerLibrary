using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Gatekeeper.LdapServerLibrary.Network;
using Microsoft.Extensions.Logging;

namespace Gatekeeper.LdapServerLibrary
{
    public class LdapServer
    {
        public int Port = 339;
        public IPAddress IPAddress = IPAddress.Parse("127.0.0.1");
        private CancellationTokenSource? cancellation;

        public void RegisterEventListener(LdapEvents ldapEvents)
        {
            SingletonContainer.SetLdapEventListener(ldapEvents);
        }

        public void RegisterLogger(ILogger logger)
        {
            SingletonContainer.SetLogger(logger);
        }

        public void RegisterCertificate(X509Certificate2 certificate)
        {
            SingletonContainer.SetCertificate(certificate);
        }

        public async Task Start()
        {
            ConnectionManager manager = new ConnectionManager();
            NetworkListener listener = new NetworkListener(
                manager,
                IPAddress,
                Port
            );
            if (cancellation != null)
                DisposeCancellationTokenSource();
            cancellation = new CancellationTokenSource();
            await listener.Start(cancellation.Token);
        }

        public void Stop()
        {
            cancellation?.Cancel();
        }

        private void DisposeCancellationTokenSource()
        {
            try
            {
                cancellation?.Dispose();
            }
            catch (Exception)
            {

            }
        }
    }
}
