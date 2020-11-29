using System;
using RtsNetworkingLibrary.client;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.server;
using RtsNetworkingLibrary.server.handlers;
using RtsNetworkingLibrary.server.utils;
using RtsNetworkingLibrary.utils;
using UnityEngine;
using Logger = RtsNetworkingLibrary.utils.Logger;

namespace RtsNetworkingLibrary.networking
{
    [RequireComponent(typeof(ServerSettings)), RequireComponent(typeof(MessageHandler))]
    public class NetworkManager : MonoBehaviour
    {
        private Server _server;
        private Client _client;
        
        private ServerSettings _serverSettings;
        private Logger _logger;
        private MessageHandler _messageHandler;
        public int ClientId { get; private set; }    // Todo set the client id after ClientConnect
        public string Username { get; set; }
        
        /*
         * Indicates if the local instance of this NetworkManager is the hosting server
         * Mostly used to check if code can be executed or not
         */
        public bool IsServer { get; private set; } = false;

        public NetworkManager()
        {
            _logger = new Logger(this.GetType().Name);
            Username = Environment.UserName;
        }

        private void Awake()
        {
            _serverSettings = GetComponent<ServerSettings>();
            _messageHandler = GetComponent<MessageHandler>();
            
            // Prevent the Networkmanager from being destroyed
            DontDestroyOnLoad(this);
            // Set logging to Unity logging, since this class is used in unity projects only
            Logger.LoggerType = LoggerType.UNITY;
            _server = new Server();
            _client = new Client();
        }

        public ServerSettings ServerSettings => _serverSettings;

        private void OnApplicationQuit()
        {
            // Close all connections
            _server.StopServer();
            _client.Disconnect();
        }


        public void StartServer()
        {
            _server.StartServer(this._serverSettings, this._messageHandler);
            IsServer = true;
        }

        public void StopServer()
        {
            _server.StopServer();
            IsServer = false;
        }

        public Server Server
        {
            get => _server;
        }

        /**
         * Sends a message to the Server
         */
        public void TcpSendToServer(NetworkMessage networkMessage)
        {
            networkMessage.userId = this.ClientId;
            networkMessage.username = this.Username;
            // TODO Send message to server
        }

        /**
         * Lets the server send a broadcast to all connected clients
         */
        public void TcpServerSendBroadcast(NetworkMessage networkMessage)
        {
            if(!IsServer)
                throw new Exception("Only the server is allowed to send broadcast messages!");
            _server.TcpBroadcast(NetworkConverter.Serialize(networkMessage));
        }

        
    }
}