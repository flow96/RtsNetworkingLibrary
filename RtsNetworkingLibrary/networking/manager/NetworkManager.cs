using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using RtsNetworkingLibrary.client;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.server;
using RtsNetworkingLibrary.server.utils;
using RtsNetworkingLibrary.unity.@base;
using RtsNetworkingLibrary.unity.callbacks;
using RtsNetworkingLibrary.utils;
using UnityEngine;
using Logger = RtsNetworkingLibrary.utils.Logger;

namespace RtsNetworkingLibrary.networking.manager
{
    [RequireComponent(typeof(ServerSettings)), RequireComponent(typeof(MessageHandler))]
    public class NetworkManager : MonoBehaviour, IServerListener
    {
        public static NetworkManager Instance;

        public string userName;
        
        private Server _server;
        private Client _client;
        
        private ServerSettings _serverSettings;
        private Logger _logger;
        private MessageHandler _messageHandler;

        private int _delay = 0;
        
        private readonly List<TransformUpdateMessage> _outBoundMessages = new List<TransformUpdateMessage>();
        private readonly Dictionary<ulong, NetworkMonoBehaviour> _spawnedObjects = new Dictionary<ulong, NetworkMonoBehaviour>();
        private List<PlayerInfo> _newPlayers = new List<PlayerInfo>();
        
        public class Test
        {
            public NetworkMonoBehaviour networkMonoBehaviour;
            public GameObject gameObject;
            public ulong entityId;

            public Test(NetworkMonoBehaviour networkMonoBehaviour, GameObject gameObject, ulong entityId)
            {
                this.networkMonoBehaviour = networkMonoBehaviour;
                this.gameObject = gameObject;
                this.entityId = entityId;
            }
        }
        
        /**
         * Indicates if the local instance of this NetworkManager is the hosting server
         * Mostly used to check if code can be executed or not
         */
        public bool IsServer { get; private set; } = false;

        public int ClientId => _client.ClientId;
        
        public ServerSettings ServerSettings => _serverSettings;

        public MessageHandler MessageHandler => _messageHandler;
        

        public NetworkManager()
        {
            _logger = new Logger(this.GetType().Name);
            Instance = this;
        }

        private void Awake()
        {
            _serverSettings = GetComponent<ServerSettings>();
            _messageHandler = GetComponent<MessageHandler>();
            
            // Prevent the Networkmanager from being destroyed
            DontDestroyOnLoad(this);
            // Set logging to Unity logging, since this class is used in unity projects only
            Logger.LoggerType = LoggerType.UNITY;
            Logger.loggingEnabled = _serverSettings.debugLogging;
            _server = new Server();
            _client = new Client();
            
            _server.AddListener(this);
        }


        private void OnApplicationQuit()
        {
            // Close all connections
            _server?.StopServer();
            _client?.Disconnect();
        }

        private void Update()
        {
            if (_outBoundMessages.Count > 0)
            {
                TransformUpdateMessage[] messages =
                    _outBoundMessages.GetRange(0, Math.Min(_outBoundMessages.Count, _serverSettings.maxHandledMessagesPerFrame)).ToArray();
                _outBoundMessages.RemoveRange(0, messages.Length);
                TransformUpdateListMessage message = new TransformUpdateListMessage(messages);
                TcpSendToServer(message);
                //_outBoundMessages.Clear();
            }

            if (_newPlayers.Count > 0)
            {
                foreach (var playerInfo in _newPlayers)
                {
                    foreach (KeyValuePair<ulong,NetworkMonoBehaviour> spawnedObject in _spawnedObjects)
                    {
                        if (spawnedObject.Value.clientId != playerInfo.userId)
                        {
                            BuildMessage buildMessage = new BuildMessage(spawnedObject.Value.prefabName,
                                NetworkHelper.ConvertToVector(spawnedObject.Value.gameObject.transform.position),
                                NetworkHelper.ConvertToVector(spawnedObject.Value.gameObject.transform.rotation.eulerAngles),
                                spawnedObject.Key);
                            buildMessage.playerInfo = new PlayerInfo(spawnedObject.Value.clientId, "");
                            _server.SendToClient(playerInfo.userId, buildMessage);   
                        }
                    }
                }
                _newPlayers.Clear();
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
            if (_outBoundMessages.Contains(message))
                _outBoundMessages.Remove(message);
            _outBoundMessages.Add(message);
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
            networkMessage.playerInfo = new PlayerInfo(this._client.ClientId, this.userName);
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


        public void OnNetworkObjectSpawned(ulong entityId, NetworkMonoBehaviour networkMonoBehaviour, GameObject gameObject)
        {
            _spawnedObjects.Add(entityId,  networkMonoBehaviour);
        }

        public void OnNetworkObjectDestroyed(ulong entityId)
        {
            _spawnedObjects.Remove(entityId);
        }

        public NetworkMonoBehaviour GetGameObjectById(ulong entityId)
        {
            return _spawnedObjects[entityId];
        }

        public void OnClientConnected(PlayerInfo playerInfo)
        {
            _logger.Debug("Sending the current objects to the new client " + _spawnedObjects.Count);
            if (!IsServer || playerInfo == null)
                throw new Exception("Illegal action! This callback is only triggered by the server!");
            _newPlayers.Add(playerInfo);
        }

        public void OnClientDisconnected(PlayerInfo playerInfo)
        {
            
        }

        public void OnServerStarted()
        {
            
        }

        public void OnServerStopped()
        {
            
        }
    }
}