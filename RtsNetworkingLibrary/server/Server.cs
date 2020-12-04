using System;
using System.Net;
using System.Net.Sockets;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.server.utils;
using RtsNetworkingLibrary.utils;

namespace RtsNetworkingLibrary.server
{
    public class Server
    {
        private readonly Logger _logger;
        
        private int clientCounter = 0;
        
        private TcpListener _server;
        private UdpClient _udpClient;

        // Clients
        private ClientHandler[] clients;
        private ServerSettings _serverSettings;
        private MessageHandler _messageHandler;

        // TODO reduce client id on client-disconnect
        
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

        public void DisconnectClient(int clientId)
        {
            clients[clientId].Disconnect();
            clients[clientId] = null;
        }

        public void RemoveClient(int clientId)
        {
            clients[clientId] = null;
        }

        public void StopServer()
        {
            if (_server != null && ServerRunning)
            {
                foreach (ClientHandler client in clients)
                {
                    client?.Disconnect("Disconnecting client, due to server stopping");
                }
                _server.Stop();
                _server.Server.Dispose();
            }
            ServerRunning = false;
            clientCounter = 0;
            _logger.Debug("Server stopped");
        }

        private void AcceptTcpClients(IAsyncResult ar)
        {
            TcpClient client = _server.EndAcceptTcpClient(ar);
            NetworkMessage message = NetworkHelper.ReceiveSingleMessage(client);
            if (message is ConnectMessage)
            {
                message.userId = clientCounter;
                NetworkHelper.SendSingleMessage(client, message, clientCounter);
                clients[clientCounter] = new ClientHandler(client, this, _serverSettings, _messageHandler, clientCounter);
                clientCounter++;
                _logger.Debug("New Client connected: " + message.userId + " = " + message.username);
            }
            else
            {
                _logger.Debug("New Client didn't send a ConnectMessage! Closing the client");
            }
            
            if(clientCounter < _serverSettings.maxPlayers)
                _server.BeginAcceptTcpClient(AcceptTcpClients, null);
        }



        public void TcpBroadcast(NetworkMessage message)
        {
            if (!ServerRunning)
            {
                throw new Exception("Server hasn't been started yet");
            }
            foreach (ClientHandler clientHandler in clients)
            {
                clientHandler?.SendTcpMessage(message);
            }
        }
        
    }
}