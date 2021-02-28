using System;
using System.Net.Sockets;
using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.server.utils;
using RtsNetworkingLibrary.utils;

namespace RtsNetworkingLibrary.server
{
    public class ClientHandler
    {

        private readonly Logger _logger;
        
        private readonly TcpClient _client;
        private readonly Server _server;
        private readonly ServerSettings _serverSettings;
        private readonly MessageHandler _messageHandler;
        private readonly NetworkStream _networkStream;
        
        public readonly PlayerInfo playerInfo;
        
        private readonly byte[] _headerBuffer;
        private byte[] _dataBuffer;
        
        private int _headerReadDelta;
        private int _dataReadDelta;
        private int _messageLength;
        
        
        public ClientHandler(TcpClient client, Server server, ServerSettings serverSettings, MessageHandler messageHandler, PlayerInfo playerInfo)
        {
            this._logger = new Logger(this.GetType().Name);
            this._client = client;
            this._server = server;
            this._networkStream = this._client.GetStream();
            this._serverSettings = serverSettings;
            this._messageHandler = messageHandler;
            this.playerInfo = playerInfo;
            this._headerBuffer = new byte[_serverSettings.headerBufferByteSize];
            _networkStream.BeginRead(_headerBuffer, 0, _headerBuffer.Length, ReadHeader, null);
        }

        private void ReadHeader(IAsyncResult ar)
        {
            int read = _networkStream.EndRead(ar);
            if (read <= 0)
            {
                Disconnect("Received empty data, socket probably disconnected! Closing connection");
                return;
            }
            _headerReadDelta += read;
            if (_headerReadDelta == _serverSettings.headerBufferByteSize) // Header complete
            {
                // Parse the data length and start reading
                _messageLength = BitConverter.ToInt32(_headerBuffer, 0);
                _dataBuffer = new byte[_messageLength];
                _networkStream.BeginRead(_dataBuffer, 0, _messageLength, ReadMessage, null);
            }
            else // Header fully received
            {
                _logger.Debug("Parts of the header missing (" + _headerReadDelta + "/" + _serverSettings.headerBufferByteSize + ")");
                _networkStream.BeginRead(_headerBuffer, _headerReadDelta,
                    _serverSettings.headerBufferByteSize - _headerReadDelta, ReadHeader, null);
            }
        }

        private void ReadMessage(IAsyncResult ar)
        {
            int read = _networkStream.EndRead(ar);
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
                // Set the player info here again, so the client can't trick the server by sending fake data
                msg.playerInfo = playerInfo;
                _messageHandler.AddServerMessage(msg);
                ResetAndWaitForNext();
            }
            else
            {
                _logger.Debug("Parts of the message missing");
                // Wait for the rest of the message
                _networkStream.BeginRead(_dataBuffer, _dataReadDelta, _messageLength - _dataReadDelta, ReadMessage,
                    null);
            }
        }

        private void ResetAndWaitForNext()
        {
            // Reset and wait for the next message
            _messageLength = 0;
            _headerReadDelta = 0;
            _dataReadDelta = 0;
            _networkStream.BeginRead(_headerBuffer, 0, _headerBuffer.Length, ReadHeader, null);
        }

        private void Disconnect(string message = "Client disconnected!")
        {
            _logger.Debug(message);
            _server.DisconnectClient(playerInfo.userId);
        }

        public void Close(string serverMessage = "Closing connection to client")
        {
            _logger.Debug(serverMessage + " , UserId: " + playerInfo.userId);
            _client.Close();
            _client.Dispose();
        }
        
        public void SendTcpMessage(NetworkMessage networkMessage)
        {
            byte[] data = NetworkConverter.Serialize(networkMessage);
            byte[] header = BitConverter.GetBytes(data.Length);
            _client.GetStream().Write(header, 0, header.Length);
            _client.GetStream().Write(data, 0, data.Length);
        }
    }
}