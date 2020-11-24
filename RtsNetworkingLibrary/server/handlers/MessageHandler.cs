using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.server.utils;
using UnityEngine;
using UnityEngine.Events;

namespace RtsNetworkingLibrary.server.handlers
{
    /**
     * This is the default parser of the Network messages.
     * Custom parsers can be added as desired in the Unity-Editor
     * The custom parsers will be executed when the default parser can't handle the type of message
     */
    public class MessageHandler : MonoBehaviour
    {
        // Public list of custom message parser
        public List<UnityEvent> customMessageParser = new List<UnityEvent>();
        
        private readonly ConcurrentQueue<NetworkMessage> _inboundMessages = new ConcurrentQueue<NetworkMessage>();
        private ServerSettings _settings;

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
                
            }
        }

        private bool handleMessage(NetworkMessage msg)
        {
            bool handled = false;
            

            return handled;
        }
        
    }
}