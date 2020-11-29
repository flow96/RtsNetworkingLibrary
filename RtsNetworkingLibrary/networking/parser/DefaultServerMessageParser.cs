using System;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.utils;
using UnityEngine;
using Logger = RtsNetworkingLibrary.utils.Logger;

namespace RtsNetworkingLibrary.networking.parser
{
    public class DefaultServerMessageParser : BaseMessageParser
    {
        private Logger _logger;
        private NetworkManager _networkManager;

        public DefaultServerMessageParser()
        {
            _logger = new Logger(this.GetType().Name);
        }

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
        }


        protected override void HandleBuildMessage(BuildMessage buildMessage)
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

        protected override void HandleDestroyMessage(DestroyMessage destroyMessage)
        {
            _logger.Debug("Handling Destroy message");
        }


        protected override void HandleDisconnectMessage(DisconnectMessage disconnectMessage)
        {
            _networkManager.Server.DisconnectClient(disconnectMessage.userId);
        }
    }
}