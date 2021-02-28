using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.server.utils;
using RtsNetworkingLibrary.unity.callbacks;
using RtsNetworkingLibrary.utils;

namespace RtsNetworkingLibrary.server
{
    public class Server
    {
        private readonly Logger _logger;
        
        private TcpListener _server;
        private ServerSettings _serverSettings;
        private MessageHandler _messageHandler;
        
        private readonly List<IServerListener> _listeners = new List<IServerListener>();
        private readonly List<ClientHandler> _clientsList = new List<ClientHandler>();

        private bool _isAccepting = false;

        
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
                // clients = new ClientHandler[settings.maxPlayers];
                ServerRunning = true;
                _server.Start(10);
                _server.BeginAcceptTcpClient(AcceptTcpClients, null);
                _isAccepting = true;
                _logger.Debug("Server started");
                _listeners.ForEach(listener => listener?.OnServerStarted());
            }
        }

        public void DisconnectClient(int clientId)
        {
            ClientHandler handler = _clientsList.Find(client => client.playerInfo.userId == clientId);
            if (handler != null)
            {
                _listeners.ForEach(listener => listener?.OnClientDisconnected(handler.playerInfo));
                handler.Close();
                _clientsList.Remove(handler);   
            }
        }


        public void StopServer()
        {
            if (_server != null && ServerRunning)
            {
                foreach (ClientHandler client in _clientsList)
                {
                    client?.Close("Disconnecting client, due to server stopping");
                }
                _clientsList.Clear();
                _server.Stop();
                _server.Server.Dispose();
            }
            ServerRunning = false;
            _listeners.ForEach(listener => listener?.OnServerStopped());
            _logger.Debug("Server stopped");
        }

        private void AcceptTcpClients(IAsyncResult ar)
        {
            TcpClient client = _server.EndAcceptTcpClient(ar);
            if (!_isAccepting)
            {
                return;
            }
            NetworkMessage message = NetworkHelper.ReceiveSingleMessage(client);
            if (message is ConnectMessage)
            {
                int userId = _clientsList.Count;
                message.playerInfo.userId = userId;
                NetworkHelper.SendSingleMessage(client, message, message.playerInfo);
                foreach (ClientHandler c in _clientsList)
                {
                    ConnectMessage msg = new ConnectMessage();
                    msg.playerInfo = c.playerInfo;
                    NetworkHelper.SendSingleMessage(client, msg, msg.playerInfo);
                }
                _clientsList.Add(new ClientHandler(client, this, _serverSettings, _messageHandler, message.playerInfo));
                _listeners.ForEach(listener => listener?.OnClientConnected(message.playerInfo));
                TcpBroadcast(message, userId);
                _logger.Debug("New Client connected: " + message.playerInfo.userId + " = " + message.playerInfo.username);
            }
            else
            {
                _logger.Debug("New Client didn't send a ConnectMessage! Closing the client");
                client.Close();
            }

            if (_clientsList.Count < _serverSettings.maxPlayers)
            {
                _isAccepting = true;
                _server.BeginAcceptTcpClient(AcceptTcpClients, null);
            }
            else
                _isAccepting = false;
        }


        public void StopAcceptingClients()
        {
            _isAccepting = false;
        }

        public void TcpBroadcast(NetworkMessage message, int exceptUserId = -1)
        {
            if (!ServerRunning)
            {
                throw new Exception("Server hasn't been started yet");
            }
            foreach (ClientHandler clientHandler in _clientsList)
            {
                if (clientHandler != null && clientHandler.playerInfo.userId != exceptUserId)
                {
                    clientHandler.SendTcpMessage(message);    
                }
            }
        }

        public void SendToClient(int clientId, NetworkMessage networkMessage)
        {
            ClientHandler client = _clientsList.Find(c => c.playerInfo.userId == clientId);
            client?.SendTcpMessage(networkMessage);
        }

        public void AddListener(IServerListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void RemoveListener(IServerListener listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }
        
    }
}