using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.networking.parser;
using RtsNetworkingLibrary.networking.utils;
using UnityEngine;

namespace RtsNetworkingLibrary.client.handlers
{
    public class DefaultClientMessageParser: BaseMessageParser
    {

        private utils.Logger _logger;
        private NetworkManager _networkManager;

        public DefaultClientMessageParser()
        {
            _logger = new utils.Logger(this.GetType().Name);
        }

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
        }

        protected override void HandleBuildMessage(BuildMessage buildMessage)
        {
            
        }

        protected override void HandleDestroyMessage(DestroyMessage message)
        {
            
        }

        protected override void HandleDisconnectMessage(DisconnectMessage message)
        {
            
        }
    }
}