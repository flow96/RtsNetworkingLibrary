using System;
using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.networking.parser;
using RtsNetworkingLibrary.unity.@base;
using RtsNetworkingLibrary.utils;
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
            _logger.Debug("Handling Build message");
            GameObject toBeSpawned = (GameObject)Resources.Load(Consts.NETWORK_PREFABS_LOCATION + buildMessage.prefabName, typeof(GameObject));
            if (toBeSpawned == null)
                throw new Exception("Could not find Prefab with name: " + buildMessage.prefabName);
            else
            {
                GameObject spawnedObject = Instantiate(toBeSpawned,
                    new Vector3(buildMessage.position.x, buildMessage.position.y, buildMessage.position.z),
                    Quaternion.Euler(buildMessage.rotation.x, buildMessage.rotation.y, buildMessage.rotation.z));
                
                NetworkMonoBehaviour networkMonoBehaviour;
                if (spawnedObject.TryGetComponent(out networkMonoBehaviour))
                {
                    spawnedObject.name = "network_object_" + buildMessage.entityId;
                    networkMonoBehaviour.clientId = buildMessage.userId;
                    networkMonoBehaviour.entityId = buildMessage.entityId;
                }
                
            }
        }

        protected override void HandleDestroyMessage(DestroyMessage message)
        {
            
        }

        protected override void HandleTransformUpdateMessage(TransformUpdate transformUpdate)
        {
            // TODO Handle Update transform
            
        }

        protected override void HandleDisconnectMessage(DisconnectMessage message)
        {
            
        }
    }
}