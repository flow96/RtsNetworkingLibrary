using System;
using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.utils;
using UnityEngine;
using Logger = RtsNetworkingLibrary.utils.Logger;

namespace RtsNetworkingLibrary.server.handlers
{
    public class DefaultMessageParser : MonoBehaviour
    {
        private Logger _logger;
        private NetworkManager _networkManager;

        public DefaultMessageParser()
        {
            _logger = new Logger(this.GetType().Name);
        }

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
        }

        public bool HandleMessage(InboundMessage message)
        {
            NetworkMessage msg = message.networkMessage;
            bool handled = true;
            if (msg is BuildMessage)
            {
                HandleBuildMessage((BuildMessage) msg, message.userId);
            }
            else if (msg is DestroyMessage)
            {
                HandleDestroyMessage((DestroyMessage) msg, message.userId);
            }
            else if (msg is DisconnectMessage)
            {
                HandleDisconnect((DisconnectMessage)msg, message.userId);
            }
            else
                handled = false;

            return handled;
        }

        private void HandleBuildMessage(BuildMessage buildMessage, int userId)
        {
            _logger.Debug("Handling Build message");
            GameObject toBeSpawned = (GameObject)Resources.Load(Consts.NETWORK_PREFABS_LOCATION + buildMessage.prefabName, typeof(GameObject));
            if (toBeSpawned == null)
                throw new Exception("Could not find Prefab with name: " + buildMessage.prefabName);
            else
            {
                Instantiate(toBeSpawned,
                    new Vector3(buildMessage.position.x, buildMessage.position.y, buildMessage.position.z),
                    Quaternion.Euler(buildMessage.rotation.x, buildMessage.rotation.y, buildMessage.rotation.z));
            }
        }

        private void HandleDestroyMessage(DestroyMessage destroyMessage, int userId)
        {
            _logger.Debug("Handling Destroy message");
        }

        private void HandleDisconnect(DisconnectMessage disconnectMessage, int userId)
        {
            if (_networkManager.IsServer)
            {
                // We are the server
                // Disconnect the client
                _networkManager.Server.DisconnectClient(userId);
            }
            else
            {
                // We are a client
                
            }
        }
    }
}