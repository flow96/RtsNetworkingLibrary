using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.networking.manager;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.unity.callbacks;
using RtsNetworkingLibrary.utils;

namespace RtsNetworkingLibrary.client
{
    public class Client
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkManager _networkManager;
        private readonly Logger _logger;
        private NetworkStream _stream;

        private readonly byte[] _headerBuffer;
        private byte[] _dataBuffer;
        
        private int _headerReadDelta;
        private int _dataReadDelta;
        private int _messageLength;

        private readonly List<IClientListener> _listeners = new List<IClientListener>();


        public int ClientId { private set; get; } = -1;

        public Client()
        {
            _logger = new Logger(this.GetType().Name);
            
            _tcpClient = new TcpClient();
            _networkManager = NetworkManager.Instance;
            _headerBuffer = new byte[_networkManager.ServerSettings.headerBufferByteSize];
        }

        public void Connect(IPEndPoint endPoint)
        {
            if (!_tcpClient.Connected)
            {
                try
                {
                    _tcpClient.Connect(endPoint);
                    _stream = _tcpClient.GetStream();
                    NetworkHelper.SendSingleMessage(_tcpClient, new ConnectMessage(Environment.UserName), -1);
                    NetworkMessage message = NetworkHelper.ReceiveSingleMessage(_tcpClient);
                    if (message is ConnectMessage)
                    {
                        ConnectMessage connectMessage = (ConnectMessage) message;
                        this.ClientId = connectMessage.playerInfo.userId;
                        _logger.Debug("Received client id: " + ClientId);
                        _stream.BeginRead(_headerBuffer, 0, _headerBuffer.Length, ReadHeader, null);
                        // Notify callbacks that we're connected
                        _listeners.ForEach(listener => listener?.OnConnected());
                    }
                    else
                    {
                        _tcpClient.Close();
                        _listeners.ForEach(listener => listener?.OnDisconnected());
                        throw new Exception("Client expected ConnectMessage, but received: " + message.GetType());
                    }
                }
                catch (Exception e)
                {
                    _logger.Debug(e);
                    if(_networkManager.IsServer)
                        _networkManager.StopServer();
                    _listeners.ForEach(listener => listener?.OnDisconnected());
                    throw new Exception("Could not connect to the server!");
                }   
            }
        }

        public void Disconnect(string message = "Closing connection to server")
        {
            if (_tcpClient.Connected)
            {
                _listeners.ForEach(listener => listener?.OnDisconnected());
                _logger.Debug(message);
                if(_tcpClient.Connected)
                    _tcpClient.Close();    
            }
        }
        

        private void ReadHeader(IAsyncResult ar)
        {
            int read = _stream.EndRead(ar);
            if (read <= 0)
            {
                Disconnect("Received empty data, socket probably disconnected! Closing connection");
                return;
            }
            _headerReadDelta += read;
            if (_headerReadDelta == _networkManager.ServerSettings.headerBufferByteSize) // Header complete
            {
                // Parse the data length and start reading
                _messageLength = BitConverter.ToInt32(_headerBuffer, 0);
                _dataBuffer = new byte[_messageLength];
                _stream.BeginRead(_dataBuffer, 0, _messageLength, ReadMessage, null);
            }
            else // Header fully reseived
            {
                _stream.BeginRead(_headerBuffer, _headerReadDelta,
                    _networkManager.ServerSettings.headerBufferByteSize - _headerReadDelta, ReadHeader, null);
            }
        }
        
        
        private void ReadMessage(IAsyncResult ar)
        {
            int read = _stream.EndRead(ar);
            if (read <= 0)
            {
                Disconnect("Received empty data, socket probably disconnected! Closing connection");
                return;
            }
            _dataReadDelta += read;
            if (_dataReadDelta == _dataBuffer.Length)
            {
                // Message fully received
                NetworkMessage msg = NetworkConverter.Deserialize(_dataBuffer);
                _networkManager.MessageHandler.AddClientMessage(msg);
                ResetAndWaitForNext();
            }
            else
            {
                // Wait for the rest of the message
                _stream.BeginRead(_dataBuffer, _dataReadDelta, _messageLength - _dataReadDelta, ReadMessage,
                    null);
            }
        }
        
        private void ResetAndWaitForNext()
        {
            // Reset and wait for the next message
            _messageLength = 0;
            _headerReadDelta = 0;
            _dataReadDelta = 0;
            _stream.BeginRead(_headerBuffer, 0, _headerBuffer.Length, ReadHeader, null);
        }

        public void SendToServer(NetworkMessage message)
        {
            if(!_tcpClient.Connected)
                throw new Exception("Client is not connected! Can't send a message to the server!");
            NetworkHelper.SendSingleMessage(_tcpClient, message, ClientId);
        }

        public void AddListener(IClientListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void RemoveListener(IClientListener listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }

    }
}