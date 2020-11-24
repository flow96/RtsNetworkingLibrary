using System;
using System.Net;
using System.Net.Sockets;
using RtsNetworkingLibrary.server.handlers;
using RtsNetworkingLibrary.server.utils;
using RtsNetworkingLibrary.utils;

namespace RtsNetworkingLibrary.server
{
    public class Server
    {
        private readonly Logger _logger;
        
        private uint clientCounter = 0;
        
        private TcpListener _server;
        private UdpClient _udpClient;

        // Clients
        private ClientHandler[] clients;
        private ServerSettings _serverSettings;
        private MessageHandler _messageHandler;

        public Server()
        {
            _logger = new Logger(this.GetType().Name);
        }

        public bool ServerRunning { private set; get; } = false;

        
        public void StartServer(ServerSettings settings, MessageHandler messageHandler)
        {
            if (!ServerRunning)
            {
                this._serverSettings = settings;
                this._messageHandler = messageHandler;
                _server = new TcpListener(IPAddress.Any, settings.port);
                clients = new ClientHandler[settings.maxPlayers];
                ServerRunning = true;
                _server.Start(10);
                _server.BeginAcceptTcpClient(AcceptTcpClients, null);
                _logger.Debug("Server started");
                //Debug.Log("Test");
            }
        }

        public void StopServer()
        {
            if (_server != null && ServerRunning)
            {
                foreach (ClientHandler client in clients)
                {
                    if(client == null)
                        continue;
                    client._client?.Close();
                    client._client?.Dispose();
                }
                _server.Stop();
                _server.Server.Dispose();
            }
            ServerRunning = false;
            clientCounter = 0;
        }

        private void AcceptTcpClients(IAsyncResult ar)
        {
            TcpClient client = _server.EndAcceptTcpClient(ar);
            clients[clientCounter++] = new ClientHandler(client, this, _serverSettings, _messageHandler);
            _logger.Debug(" >> Server: New Client connected");
            if(clientCounter < _serverSettings.maxPlayers)
                _server.BeginAcceptTcpClient(AcceptTcpClients, null);
        }

    }
}