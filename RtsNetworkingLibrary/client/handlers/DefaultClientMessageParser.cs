using System;
using RtsNetworkingLibrary.networking.manager;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.networking.messages.game;
using RtsNetworkingLibrary.networking.parser;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.unity.@base;
using RtsNetworkingLibrary.utils;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        protected override void HandleClientConnectMessage(ConnectMessage connectMessage)
        {
            if(connectMessage.playerInfo.userId != _networkManager.ClientId)
                _networkManager.Client.Listeners.ForEach(l => l.OtherPlayerConnected(connectMessage.playerInfo));
        }

        protected override void HandleBuildMessage(BuildMessage buildMessage)
        {
            _logger.Debug("Handling Build message");
            GameObject toBeSpawned = (GameObject)Resources.Load(Consts.NETWORK_PREFABS_LOCATION + buildMessage.prefabName, typeof(GameObject));
            if (toBeSpawned == null)
                throw new Exception("Could not find Prefab with name: " + buildMessage.prefabName);
            
            GameObject spawnedObject = Instantiate(toBeSpawned,
                new Vector3(buildMessage.position.x, buildMessage.position.y, buildMessage.position.z),
                Quaternion.Euler(buildMessage.rotation.x, buildMessage.rotation.y, buildMessage.rotation.z));
            
            NetworkMonoBehaviour networkMonoBehaviour;
            if (spawnedObject.TryGetComponent(out networkMonoBehaviour))
            {
                networkMonoBehaviour.clientId = buildMessage.playerInfo.userId;
                networkMonoBehaviour.entityId = buildMessage.entityId;
                networkMonoBehaviour.prefabName = buildMessage.prefabName;
                _networkManager.OnNetworkObjectSpawned(networkMonoBehaviour.entityId, networkMonoBehaviour, spawnedObject);
            }

            if (buildMessage.callbackHashCode != 0)
            {
                if(_networkManager.instantiateCallbacks.ContainsKey(buildMessage.callbackHashCode))
                {
                    _networkManager.instantiateCallbacks[buildMessage.callbackHashCode](spawnedObject);
                    _networkManager.instantiateCallbacks.Remove(buildMessage.callbackHashCode);
                }
            }
        }

        protected override void HandleDestroyMessage(DestroyMessage message)
        {
            Destroy(_networkManager.GetGameObjectById(message.entityId));
            _networkManager.OnNetworkObjectDestroyed(message.entityId);
        }

        protected override void HandleTransformUpdateMessage(TransformUpdateMessage transformUpdateMessage)
        {
            _logger.Debug("Received transform update");
            GameObject toBeUpdated = _networkManager.GetGameObjectById(transformUpdateMessage.entityId)?.gameObject;
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
            GameObject toBeUpdated = _networkManager.GetGameObjectById(updateSyncVarMessage.entityId)?.gameObject;
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
            if (message.playerInfo.userId == _networkManager.ClientId)
            {
                _networkManager.Client.Disconnect("Reicved close event from server -> Closing connection.");
            }
            else
            {
                _networkManager.Client.Listeners.ForEach(l => l.OtherPlayerDisconnected(message.playerInfo));
            }
        }

        protected override void HandleStartGameMessage(StartGameMessage message)
        {
            SceneManager.LoadScene(message.sceneName);
        }
    }
}