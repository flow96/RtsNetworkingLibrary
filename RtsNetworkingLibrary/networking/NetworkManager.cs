using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using RtsNetworkingLibrary.client;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.server;
using RtsNetworkingLibrary.server.utils;
using RtsNetworkingLibrary.unity.callbacks;
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

        private int _delay = 0;
        
        public string Username { get; set; }
        
        /*
         * Indicates if the local instance of this NetworkManager is the hosting server
         * Mostly used to check if code can be executed or not
         */
        public bool IsServer { get; private set; } = false;

        public int ClientId => _client.ClientId;
        
        public ServerSettings ServerSettings => _serverSettings;

        public MessageHandler MessageHandler => _messageHandler;
        
        private readonly ConcurrentQueue<TransformUpdateMessage> _outBoundMessages = new ConcurrentQueue<TransformUpdateMessage>();

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
            _client = new Client(this);
        }


        private void OnApplicationQuit()
        {
            // Close all connections
            _server?.StopServer();
            _client?.Disconnect();
        }

        private void Update()
        {
            --_delay;
            if (_delay <= 0)
            {
                _delay = 60 / ServerSettings.sendUpdateThresholdPerSecond;
                List<TransformUpdateMessage> msgs = new List<TransformUpdateMessage>();
                for (int i = 0; i < _serverSettings.maxHandledMessagesPerFrame && i < _outBoundMessages.Count; i++)
                {
                    if(_outBoundMessages.TryDequeue(out var outMsg))
                        msgs.Add(outMsg);
                }

                if (msgs.Count > 0)
                {
                    TransformUpdateListMessage message = new TransformUpdateListMessage(msgs.ToArray());
                
                    TcpSendToServer(message);
                }
                    
            }
        }

        public void StartServer()
        {
            _server.StartServer(this._serverSettings, this._messageHandler);
            IsServer = true;
            IPAddress address = Array.FindLast(
                Dns.GetHostEntry(string.Empty).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint endPoint = new IPEndPoint(address, _serverSettings.port);
            if (address == null)
            {
                throw new Exception("Could not start the server, because your local IP could not be fetched (therefore the local client can't connect)");
            }
            ConnectToServer(endPoint);
        }

        public void StopServer()
        {
            _server.StopServer();
            IsServer = false;
        }

        /**
         * Connects a client to a specific server ip
         */
        public void ConnectToServer(IPEndPoint endPoint)
        {
            _client.Connect(endPoint);
        }

        /**
         * Connects a client to the server, defined in the server settings
         */
        public void ConnectClient()
        {
            ConnectToServer(new IPEndPoint(IPAddress.Parse(_serverSettings.serverIp), _serverSettings.port));
        }

        public void AddClientListener(IClientListener listener)
        {
            _client.AddListener(listener);
        }

        public void RemoveClientListener(IClientListener listener)
        {
            _client.RemoveListener(listener);
        }

        public void AddServerListener(IServerListener listener)
        {
            _server.AddListener(listener);
        }
        
        public void RemoveServerListener(IServerListener listener)
        {
            _server.RemoveListener(listener);
        }

        public void EnqueOutboundUpdateMessage(TransformUpdateMessage message)
        {
            TransformUpdateMessage old;
            if (_outBoundMessages.Contains(message))
                _outBoundMessages.TryDequeue(out old);
            old = null;
            _outBoundMessages.Enqueue(message);
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
            _logger.Debug("Sending a message of type: " + networkMessage.GetType());
            networkMessage.playerInfo = new PlayerInfo(this._client.ClientId, this.Username);
            _client.SendToServer(networkMessage);
        }

        /**
         * Lets the server send a broadcast to all connected clients
         */
        public void TcpServerSendBroadcast(NetworkMessage networkMessage, int exceptUserId = -1)
        {
            if(!IsServer)
                throw new Exception("Only the server is allowed to send broadcast messages!");
            _server.TcpBroadcast(networkMessage, exceptUserId);
        }

        public void Instantiate(string assetPrefabName, Vector3 position = new Vector3(),
            Quaternion rotation = new Quaternion())
        {
            Vector3 euler = rotation.eulerAngles;
            BuildMessage buildMessage = new BuildMessage(assetPrefabName, new Vector(position.x, position.y, position.z), new Vector(euler.x, euler.y, euler.z));
            TcpSendToServer(buildMessage);
        }
    }
}