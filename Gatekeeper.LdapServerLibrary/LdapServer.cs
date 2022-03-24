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
        private readonly ConnectionManager manager;
        private NetworkListener? listener;
        private CancellationTokenSource? cancellation;

        public LdapServer()
        {
            manager = new ConnectionManager();
        }

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

        public async Task Start()
        {
            if (IsRunning) // server already running
                return;
            if (listener == null)
            {
                listener = new NetworkListener(
                    manager,
                    IPAddress,
                    Port);
            }
            if (cancellation != null)
                DisposeCancellationTokenSource();
            cancellation = new CancellationTokenSource();
            await listener.Start(cancellation.Token);
        }

        public void Stop()
        {
            cancellation?.Cancel();
        }

        public bool IsRunning => listener?.IsRunning ?? false;

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
