using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Gatekeeper.LdapServerLibrary.Network
{
    internal class NetworkListener
    {
        private readonly ConnectionManager _connectionManager;
        private readonly int _port;
        private readonly IPAddress _ipAddress;
        private TcpListener? _listener;
        private bool _isRunning = false;

        internal NetworkListener(
            ConnectionManager connectionManager, 
            IPAddress ipAddress,
            int port)
        {
            _connectionManager = connectionManager;
            _ipAddress = ipAddress;
            _port = port;
        }

        internal async Task Start(CancellationToken cancellationToken)
        {
            ILogger? logger = SingletonContainer.GetLogger();
            cancellationToken.Register(StopTriggered);
            try
            {
                _isRunning = true;
                if (_listener == null)
                    _listener = new TcpListener(_ipAddress, _port);
                _listener.Start();
                while (!cancellationToken.IsCancellationRequested)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _connectionManager.AddClient(client, cancellationToken);
                }
                logger?.LogInformation("TcpListener has come to an end");
            }
            catch (Exception e)
            {
                if (e is SocketException s)
                {
                    if (s.SocketErrorCode == SocketError.OperationAborted)
                        logger?.LogInformation("Socket operation aborted, assuming TcpListener was stopped");
                    else
                        logger?.LogError(e, "Other socket error");
                }
                else
                    logger?.LogError(e, "Exception dealing with inbound requests");
            }
        }

        private void StopTriggered()
        {
            _listener?.Stop();
            _isRunning = false;
        }

        internal bool IsRunning => _isRunning;
    }
}
