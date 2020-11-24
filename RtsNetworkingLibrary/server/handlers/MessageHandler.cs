using System.Collections.Concurrent;
using System.Collections.Generic;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.server.utils;
using UnityEngine;

namespace RtsNetworkingLibrary.server.handlers
{
    /**
     * This is the main handler for the Network messages.
     * Custom parsers can be added as desired in the Unity-Editor
     * The custom parsers will be executed when the default parser can't handle the type of message
     */
    public class MessageHandler : MonoBehaviour
    {
        // Public list of custom message parser
        public List<CustomMessageParser> customMessageParser = new List<CustomMessageParser>();
        
        private RtsNetworkingLibrary.utils.Logger _logger;
        
        private readonly ConcurrentQueue<NetworkMessage> _inboundMessages = new ConcurrentQueue<NetworkMessage>();
        private readonly DefaultMessageParser _defaultMessageParser = new DefaultMessageParser();
        private ServerSettings _settings;

        MessageHandler()
        {
            _logger = new RtsNetworkingLibrary.utils.Logger(this.GetType().Name);
        }
        
        private void Start()
        {
            this._settings = GetComponent<ServerSettings>();
        }

        public void addMessage(NetworkMessage msg)
        {
            _inboundMessages.Enqueue(msg);
        }

        private void Update()
        {
            byte handledMessages = 0; // Handling max < 255 messages per frame
            while (!_inboundMessages.IsEmpty && handledMessages++ < _settings.maxHandledMessagesPerFrame)
            {
                NetworkMessage message;
                _inboundMessages.TryDequeue(out message);
                if (!_defaultMessageParser.HandleMessage(message))
                {
                    // Message was not handled by default handler, try calling a custom one
                    foreach (CustomMessageParser parser in customMessageParser)
                    {
                        parser.Invoke(message);
                    }
                }
            }
        }
        
    }
}