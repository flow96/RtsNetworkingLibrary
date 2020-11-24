using System;
using System.Net.Sockets;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.server.utils;
using RtsNetworkingLibrary.utils;

namespace RtsNetworkingLibrary.server
{
    public class ClientHandler
    {

        private readonly Logger _logger;
        
        public readonly TcpClient _client;
        public readonly Server _server;
        private readonly ServerSettings _serverSettings;
        
        private readonly NetworkStream _networkStream;
        
        private readonly byte[] _headerBuffer;
        private byte[] _dataBuffer;
        
        private int _headerReadDelta;
        private int _dataReadDelta;
        private int _messageLength;
        
        
        public ClientHandler(TcpClient client, Server server, ServerSettings serverSettings)
        {
            this._logger = new Logger(this.GetType().Name);
            this._client = client;
            this._server = server;
            this._networkStream = this._client.GetStream();
            this._serverSettings = serverSettings;
            this._headerBuffer = new byte[_serverSettings.headerBufferByteSze];
            
            _networkStream.BeginRead(_headerBuffer, 0, _headerBuffer.Length, ReadHeader, null);
        }

        private void ReadHeader(IAsyncResult ar)
        {
            int read = _networkStream.EndRead(ar);
            if (read <= 0)
            {
                HandleDisconnect("Received empty data, socket probably disconnected! Closing connection");
                return;
            }
            _headerReadDelta += read;
            if (_headerReadDelta == _serverSettings.headerBufferByteSze) // Header complete
            {
                _logger.Debug("Header fully received");
                // Parse the data length and start reading
                _messageLength = BitConverter.ToInt32(_headerBuffer, 0);
                _dataBuffer = new byte[_messageLength];
                _networkStream.BeginRead(_dataBuffer, 0, _messageLength, ReadMessage, null);
            }
            else // Header fully reseived
            {
                _logger.Debug("Parts of the header missing (" + _headerReadDelta + "/" + _serverSettings.headerBufferByteSze + ")");
                _networkStream.BeginRead(_headerBuffer, _headerReadDelta,
                    _serverSettings.headerBufferByteSze - _headerReadDelta, ReadHeader, null);
            }
        }

        private void ReadMessage(IAsyncResult ar)
        {
            int read = _networkStream.EndRead(ar);
            if (read <= 0)
            {
                HandleDisconnect("Received empty data, socket probably disconnected! Closing connection");
                return;
            }

            _dataReadDelta += read;
            if (_dataReadDelta == _dataBuffer.Length)
            {
                // Message fully received
                // TODO Handle message
                _logger.Debug("Message fully received");
                RawDataMessage rawDataMessage = new RawDataMessage(_dataBuffer);
                NetworkMessage msg = NetworkConverter.Deserialize(rawDataMessage);
                _logger.Debug("Message is type of: " + msg.GetType());
                if (msg is BuildMessage)
                {
                    BuildMessage buildMessage = (BuildMessage) msg;
                    _logger.Debug("Prefab: " + buildMessage.prefabName);
                }
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
            _logger.Debug("Waiting for next message");
        }

        private void HandleDisconnect(string message = "Client disconnected!")
        {
            _logger.Debug(message);
            _client.Close();
        }
        
    }
}