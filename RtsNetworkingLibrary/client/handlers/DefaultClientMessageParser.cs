using System;
using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.networking.parser;
using RtsNetworkingLibrary.networking.utils;
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
                    spawnedObject.name = Consts.NETWORK_OBJECT_PREFIX + buildMessage.entityId;
                    networkMonoBehaviour.clientId = buildMessage.playerInfo.userId;
                    networkMonoBehaviour.entityId = buildMessage.entityId;
                }
                
            }
        }

        protected override void HandleDestroyMessage(DestroyMessage message)
        {
            
        }

        protected override void HandleTransformUpdateMessage(TransformUpdateMessage transformUpdateMessage)
        {
            _logger.Debug("Received transform update");
            GameObject toBeUpdated = GameObject.Find(Consts.NETWORK_OBJECT_PREFIX + transformUpdateMessage.entityId);
            if (toBeUpdated != null)
            {
                NetworkMonoBehaviour networkMonoBehaviour;
                if (toBeUpdated.TryGetComponent(out networkMonoBehaviour))
                {
                    networkMonoBehaviour.SetNextTransform(NetworkHelper.ConvertToVector3(transformUpdateMessage.position), NetworkHelper.ConvertToVector3(transformUpdateMessage.rotation));
                }
                else
                {
                    throw new Exception("The Network object with id: " + transformUpdateMessage.entityId + " has no NetworkMonoBehaviour component on it, thus it can't be updated!");
                }
            }
            else
            {
                throw new Exception("Network object with id: " + transformUpdateMessage.entityId + " could not be found!");
            }
        }

        protected override void HandleTransformUpdateListMessage(TransformUpdateListMessage transformUpdateListMessage)
        {
            foreach (TransformUpdateMessage updateMessage in transformUpdateListMessage.updates)
            {
                HandleTransformUpdateMessage(updateMessage);
            }
        }

        protected override void HandleUpdateSyncVarMessage(UpdateSyncVarMessage updateSyncVarMessage)
        {
            _logger.Debug("Received Update Sync Var Message");
            GameObject toBeUpdated = GameObject.Find(Consts.NETWORK_OBJECT_PREFIX + updateSyncVarMessage.entityId);
            if (toBeUpdated == null)
            {
                throw new Exception("Could not find Network Object with id: " + updateSyncVarMessage.entityId + " to update sync vars");
            }

            NetworkMonoBehaviour networkMonoBehaviour;
            if (toBeUpdated.TryGetComponent(out networkMonoBehaviour))
            {
                networkMonoBehaviour.UpdateSyncVars(updateSyncVarMessage.data);
            }
            else
            {
                throw new Exception("The GameObject with network id: " + updateSyncVarMessage.entityId + " has no NetworkMonoBehaviour Script on it!");
            }
        }

        protected override void HandleDisconnectMessage(DisconnectMessage message)
        {
            
        }
    }
}