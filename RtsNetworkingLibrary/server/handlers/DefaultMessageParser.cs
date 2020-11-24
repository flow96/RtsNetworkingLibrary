using System;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.utils;
using UnityEngine;
using Logger = RtsNetworkingLibrary.utils.Logger;

namespace RtsNetworkingLibrary.server.handlers
{
    public class DefaultMessageParser : MonoBehaviour
    {
        private Logger _logger;

        public DefaultMessageParser()
        {
            _logger = new Logger(this.GetType().Name);
        }
        
        public bool HandleMessage(NetworkMessage msg)
        {
            bool handled = true;
            if (msg is BuildMessage)
            {
                HandleBuildMessage((BuildMessage) msg);
            }
            else if (msg is DestroyMessage)
            {
                HandleDestroyMessage((DestroyMessage) msg);
            }
            else
                handled = false;

            return handled;
        }

        private void HandleBuildMessage(BuildMessage buildMessage)
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

        private void HandleDestroyMessage(DestroyMessage destroyMessage)
        {
            _logger.Debug("Handling Destroy message");
        }
    }
}