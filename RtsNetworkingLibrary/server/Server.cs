using System;
using System.Net;
using System.Net.Sockets;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.server.handlers;
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
            NetworkMessage message = ReceiveSingleMessage(client);
            if (message is ConnectMessage)
            {
                ConnectMessage connectMessage = (ConnectMessage) message;
                connectMessage.userId = clientCounter;
                SendSingleMessage(client, connectMessage);
                clients[clientCounter] = new ClientHandler(client, this, _serverSettings, _messageHandler, clientCounter);
                clientCounter++;
                _logger.Debug("New Client connected: " + connectMessage.userId + " = " + connectMessage.username);
            }
            else
            {
                _logger.Debug("New Client didn't send a ConnectMessage! Closing the client");
            }
            
            if(clientCounter < _serverSettings.maxPlayers)
                _server.BeginAcceptTcpClient(AcceptTcpClients, null);
        }

        private NetworkMessage ReceiveSingleMessage(TcpClient client)
        {
            byte[] dataBuffer = new byte[4];
            int headerLength = 0, msgDataLength = 0, msgReadLength = 0;
            do
            {
                headerLength += client.GetStream().Read(dataBuffer, 0, 4 - headerLength);    
            } while (headerLength < 4);
            msgDataLength = BitConverter.ToInt32(dataBuffer, 0);
            dataBuffer = new byte[msgDataLength];
            do
            {
                msgReadLength += client.GetStream().Read(dataBuffer, 0, msgDataLength - msgReadLength);
            } while (msgReadLength < msgDataLength);
            return (NetworkConverter.Deserialize(dataBuffer));
        }

        private void SendSingleMessage(TcpClient client, NetworkMessage message)
        {
            byte[] data = NetworkConverter.Serialize(message);
            byte[] header = BitConverter.GetBytes(data.Length);
            client.GetStream().Write(header, 0, header.Length);
            client.GetStream().Write(data, 0, data.Length);
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