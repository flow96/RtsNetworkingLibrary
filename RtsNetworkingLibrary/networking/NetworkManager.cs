using System;
using RtsNetworkingLibrary.client;
using RtsNetworkingLibrary.server;
using RtsNetworkingLibrary.server.handlers;
using RtsNetworkingLibrary.server.utils;
using UnityEngine;

namespace RtsNetworkingLibrary.networking
{
    [RequireComponent(typeof(ServerSettings)), RequireComponent(typeof(MessageHandler))]
    public class NetworkManager : MonoBehaviour
    {
        private Server _server;
        private Client _client;
        
        private ServerSettings _serverSettings;
        
        private void Awake()
        {
            _serverSettings = GetComponent<ServerSettings>();
        }

        private void Start()
        {
            // Prevent the Networkmanager from being destroyed
            DontDestroyOnLoad(this);
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
        }

        public void StopServer()
        {
            _server.StopServer();
        }

        public Server Server
        {
            get => _server;
        }
    }
}