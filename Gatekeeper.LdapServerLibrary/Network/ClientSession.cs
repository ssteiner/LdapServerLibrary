using Gatekeeper.LdapServerLibrary.Engine;
using Gatekeeper.LdapServerLibrary.Engine.Handler;
using Gatekeeper.LdapServerLibrary.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Gatekeeper.LdapServerLibrary.Network
{
    internal class ClientSession
    {
        internal readonly TcpClient Client;
        internal readonly CancellationToken CancellationToken;
        private bool _useStartTls;
        private bool _clientIsConnected = true;

        private const int ASN_LENGTH_INDICATOR = 1;
        private const int ASN_MAX_SINGLE_BYTE_LENGTH = 127;
        private const int ASN_LENGTH_PREFIX_COUNT = 2;

        internal ClientSession(TcpClient client, CancellationToken cancellationToken)
        {
            Client = client;
            CancellationToken = cancellationToken;
            cancellationToken.Register(TokenAborted);
        }

        private int GetMultiByteLength(byte lengthIndicator)
        {
            return (lengthIndicator >> 0) & 127;
        }

        public async Task<byte[]> ReadFullyAsync(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                List<byte> PacketLength = new List<byte>();
                byte[] LengthBuffer = new byte[10];
                int streamPosition = 0;
                int? packetSize = null;
                bool isMultiByteSize = false;
                int? multiByteSize = null;

                while (true)
                {
                    byte[] buffer = new byte[1];
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken);

                    if (streamPosition == ASN_LENGTH_INDICATOR)
                    {
                        int number = Convert.ToInt32(buffer[0]);
                        if (number <= ASN_MAX_SINGLE_BYTE_LENGTH)
                        {
                            packetSize = number + ASN_LENGTH_PREFIX_COUNT;
                        }
                        else
                        {
                            isMultiByteSize = true;
                            multiByteSize = GetMultiByteLength(buffer[0]);
                        }
                    }
                    else
                    {
                        if (isMultiByteSize && (streamPosition - ASN_LENGTH_PREFIX_COUNT) < multiByteSize)
                        {
                            PacketLength.Add(buffer[0]);
                        }
                        else if (isMultiByteSize && (streamPosition - ASN_LENGTH_PREFIX_COUNT) == multiByteSize)
                        {
                            string hexValue = BitConverter.ToString(PacketLength.ToArray()).Replace("-", "");
                            packetSize = Convert.ToInt32(hexValue, 16) + ASN_LENGTH_PREFIX_COUNT + PacketLength.Count;
                        }
                    }

                    ms.Write(buffer, 0, read);
                    streamPosition++;

                    if (read <= 0 || streamPosition == packetSize)
                    {
                        return ms.ToArray();
                    }
                }
            }
        }

        internal void StartReceiving()
        {
            Task networkTask = new Task(async () =>
            {
                NetworkStream unencryptedStream = Client.GetStream();
                SslStream sslStream = new SslStream(unencryptedStream);

                IPEndPoint? endpoint = (IPEndPoint?)Client.Client.RemoteEndPoint;
                if (endpoint == null)
                {
                    throw new Exception("IP address is null");
                }

                ClientContext clientContext = new ClientContext(endpoint.Address);
                DecisionEngine engine = new DecisionEngine(clientContext);

                bool _initializedTls = false;

                while (_clientIsConnected)
                {
                    Stream rawOrSslStream = (_useStartTls) ? sslStream : unencryptedStream;

                    try
                    {
                        if (_useStartTls && !_initializedTls)
                        {
                            await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                            {
                                ServerCertificate = SingletonContainer.GetCertificate(),
                            });
                            _initializedTls = true;
                        }

                        byte[] data = await ReadFullyAsync(rawOrSslStream);

                        await HandleAsync(data, rawOrSslStream, engine);
                    }
                    catch (Exception e)
                    {
                        if (e is OperationCanceledException) // read operation aborted => server shutdown
                        {

                        }
                        else if (e is IOException i)
                        {
                            if (i.InnerException != null && i.InnerException is SocketException s)
                            {
                                if (s.SocketErrorCode != SocketError.ConnectionReset)
                                {
                                    ILogger? logger = SingletonContainer.GetLogger();
                                    logger?.LogWarning($"Socket exception dealing with request from {endpoint}. Error code: {s.SocketErrorCode}. Message: {s.Message}");
                                }
                            }
                            else
                            {
                                ILogger? logger = SingletonContainer.GetLogger();
                                logger?.LogError(e, $"IOException handling request from {endpoint}");
                            }
                        }
                        else
                        {
                            ILogger? logger = SingletonContainer.GetLogger();
                            logger?.LogError(e, $"Exception handling request from {endpoint}");
                        }
                    }
                }

                Client.Close();
            });

            networkTask.Start();
        }

        private async Task HandleAsync(byte[] bytes, Stream stream, DecisionEngine engine)
        {
            LdapPacketParserLibrary.Parser parser = new LdapPacketParserLibrary.Parser();
            LdapPacketParserLibrary.Models.LdapMessage message = parser.TryParsePacket(bytes);

            List<LdapMessage> replies = await engine.GenerateReply(message);
            foreach (LdapMessage outMsg in replies)
            {

                if (outMsg.ProtocolOp.GetType() == typeof(Models.Operations.Response.UnbindDummyResponse) 
                    || outMsg.ProtocolOp.GetType() == typeof(Models.Operations.Response.AbandonResponse))
                {
                    _clientIsConnected = false;
                    break;
                }
                byte[] msg = new Parser.PacketParser().TryEncodePacket(outMsg);
                stream.Write(msg, 0, msg.Length);

                if (outMsg.ProtocolOp.GetType() == typeof(Models.Operations.Response.ExtendedOperationResponse))
                {
                    var response = (Models.Operations.Response.ExtendedOperationResponse)outMsg.ProtocolOp;
                    if (response.ResponseName == ExtendedRequestHandler.StartTLS)
                    {
                        _useStartTls = true;
                    }
                }
            }
        }

        private void TokenAborted()
        {
            _clientIsConnected = false;
        }
    }
}
