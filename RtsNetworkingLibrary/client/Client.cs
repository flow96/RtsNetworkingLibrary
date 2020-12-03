using System;
using System.Net;
using System.Net.Sockets;
using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.utils;

namespace RtsNetworkingLibrary.client
{
    public class Client
    {
        private TcpClient _tcpClient;
        private readonly NetworkManager _networkManager;
        private readonly Logger _logger;
        private NetworkStream _stream;

        private byte[] _headerBuffer;
        private byte[] _dataBuffer;
        
        private int _headerReadDelta;
        private int _dataReadDelta;
        private int _messageLength;

        public int ClientId { private set; get; } = -1;

        public Client(NetworkManager networkManager)
        {
            _logger = new Logger(this.GetType().Name);
            
            _tcpClient = new TcpClient();
            _networkManager = networkManager;
            _headerBuffer = new byte[networkManager.ServerSettings.headerBufferByteSize];
        }

        public void Connect(IPEndPoint endPoint)
        {
            if (!_tcpClient.Connected)
            {
                try
                {
                    _tcpClient.Connect(endPoint);
                    _stream = _tcpClient.GetStream();
                    ReadHandshake();
                }
                catch (Exception e)
                {
                    _logger.Debug(e);
                    if(_networkManager.IsServer)
                        _networkManager.StopServer();
                    throw new Exception("Could not connect to the server!");
                }   
            }
        }
        
        public void Disconnect(string message = "Closing connection to server")
        {
            _logger.Debug(message);
            if(_tcpClient.Connected)
                _tcpClient.Close();
        }


        private void ReadHandshake()
        {
            
            byte[] connectMsg = new byte[4];
            int headerLength = 0;
            int msgDataLength = 0, msgReadLength = 0;
            do
            {
                headerLength += _stream.Read(connectMsg, 0, _networkManager.ServerSettings.headerBufferByteSize - headerLength);    
            } while (headerLength < 4);
            msgDataLength = BitConverter.ToInt32(connectMsg, 0);
            connectMsg = new byte[msgDataLength];
            do
            {
                msgReadLength += _stream.Read(connectMsg, 0, msgDataLength - msgReadLength);
            } while (msgReadLength < msgDataLength);
            RawDataMessage rawDataMessage = new RawDataMessage(connectMsg);
            NetworkMessage msg = NetworkConverter.Deserialize(rawDataMessage);
            if (msg is ConnectMessage)
            {
                ConnectMessage connectMessage = (ConnectMessage) msg;
                this.ClientId = connectMessage.userId;
                _logger.Debug("Received client id: " + ClientId);
                _stream.BeginRead(_headerBuffer, 0, _headerBuffer.Length, ReadHeader, null);
            }
            else
            {
                _tcpClient.Close();
                throw new Exception("Client expected ConnectMessage, but received: " + msg.GetType());
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
                RawDataMessage rawDataMessage = new RawDataMessage(_dataBuffer);
                NetworkMessage msg = NetworkConverter.Deserialize(rawDataMessage);
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

        public void SendToServer(NetworkMessage networkMessage)
        {
            RawDataMessage rMessage = NetworkConverter.Serialize(networkMessage);
            byte[] header = BitConverter.GetBytes(rMessage.data.Length);
            _stream.Write(header, 0, header.Length);
            _stream.Write(rMessage.data, 0, rMessage.data.Length);
        }
    }
}