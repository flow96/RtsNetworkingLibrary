using System;
using RtsNetworkingLibrary.client;
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
        
        /*
         * Indicates if the local instance of this NetworkManager is the hosting server
         * Mostly used to check if code can be executed or not
         */
        public bool IsServer { get; private set; } = false;

        public NetworkManager()
        {
            _logger = new Logger(this.GetType().Name);
        }

        private void Awake()
        {
            _serverSettings = GetComponent<ServerSettings>();
        }

        private void Start()
        {
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
            _server.StartServer(this._serverSettings);
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
    }
}