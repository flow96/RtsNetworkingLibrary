using System.Collections.Concurrent;
using System.Collections.Generic;
using RtsNetworkingLibrary.client.handlers;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.parser;
using RtsNetworkingLibrary.server.handlers;
using RtsNetworkingLibrary.server.utils;
using UnityEngine;

namespace RtsNetworkingLibrary.networking.utils
{
    /**
     * This is the main handler for the Network messages.
     * Custom parsers can be added as desired in the Unity-Editor
     * The custom parsers will be executed when the default parser can't handle the type of message
     */
    public class MessageHandler : MonoBehaviour
    {
        // Public list of custom message parser
        public List<CustomMessageParser> customServerMessageParser = new List<CustomMessageParser>();
        public List<CustomMessageParser> customClientMessageParser = new List<CustomMessageParser>();
        
        private readonly RtsNetworkingLibrary.utils.Logger _logger;
        
        private readonly ConcurrentQueue<NetworkMessage> _inboundServerMessages = new ConcurrentQueue<NetworkMessage>();
        private readonly ConcurrentQueue<NetworkMessage> _inboundClientMessages = new ConcurrentQueue<NetworkMessage>();
        private BaseMessageParser _defaultServerMessageParser;
        private BaseMessageParser _defaultClientMessageParser;
        private ServerSettings _settings;
        private NetworkManager _networkManager;

        MessageHandler()
        {
            _logger = new RtsNetworkingLibrary.utils.Logger(this.GetType().Name);
        }

        private void Awake()
        {
            _defaultServerMessageParser = gameObject.AddComponent<DefaultServerMessageParser>();
            _defaultClientMessageParser = gameObject.AddComponent<DefaultClientMessageParser>();
        }

        private void Start()
        {
            this._networkManager = GetComponent<NetworkManager>();
            this._settings = GetComponent<ServerSettings>();
        }

        /**
         * Adds a message to the list, that has been sent by a client and should be processed by the server
         */
        public void AddServerMessage(NetworkMessage message)
        {
            if(_networkManager.IsServer)
                _inboundServerMessages.Enqueue(message);
        }

        /**
         * Adds a message to the list, that has been sent by the server and should be processed by the client
         */
        public void AddClientMessage(NetworkMessage message)
        {
            _inboundClientMessages.Enqueue(message);
        }

        private void Update()
        {
            int handledMessages = 0;
            // Handle Server messages
            while (!_inboundServerMessages.IsEmpty && handledMessages++ < _settings.maxHandledMessagesPerFrame)
            {
                NetworkMessage message;
                _inboundServerMessages.TryDequeue(out message);
                if (!_defaultServerMessageParser.ParseMessage(message))
                {
                    _logger.Debug("Message of type: " + message.GetType() + " was not handled, sending it to custom server parser");
                    // Message was not handled by default handler, try calling a custom one
                    foreach (CustomMessageParser parser in customServerMessageParser)
                    {
                        parser.Invoke(message);
                    }
                }
            }
            handledMessages = 0;
            // Handle Client messages
            while (!_inboundClientMessages.IsEmpty && handledMessages++ < _settings.maxHandledMessagesPerFrame)
            {
                NetworkMessage message;
                _inboundClientMessages.TryDequeue(out message);
                if (!_defaultClientMessageParser.ParseMessage(message))
                {
                    _logger.Debug("Message of type: " + message.GetType() + " was not handled, sending it to custom client parser");
                    // Message was not handled by default handler, try calling a custom one
                    foreach (CustomMessageParser parser in customClientMessageParser)
                    {
                        parser.Invoke(message);
                    }
                }
            }
        }
        
    }
}