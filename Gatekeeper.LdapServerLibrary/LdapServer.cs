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
        public IPAddress IPAddress = IPAddress.Loopback;
        private CancellationTokenSource? cancellation;

        public void RegisterEventListener(ILdapEvents ldapEvents)
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

        public async Task Start(IPAddress? address = null)
        {
            if (cancellation?.IsCancellationRequested == false) // server is already started
                return;
            if (address == null)
                address = IPAddress.Loopback;
            ConnectionManager manager = new ConnectionManager();
            NetworkListener listener = new NetworkListener(
                manager,
                address ?? IPAddress,
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

        public bool IsRunning => cancellation?.IsCancellationRequested == false;

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
