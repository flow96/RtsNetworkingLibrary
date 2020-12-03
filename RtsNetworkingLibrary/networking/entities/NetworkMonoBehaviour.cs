using System;
using UnityEngine;

namespace RtsNetworkingLibrary.networking.entities
{
    public abstract class NetworkMonoBehaviour : MonoBehaviour
    {

        private NetworkManager _networkManager;

        private void Start()
        {
            this._networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            if (_networkManager == null)
            {
                throw new Exception(
                    "Network Manager not found! You should have a GameObject called \"NetworkManager\" in your project, that has the NetworkManager script on it!");
            }
        }

        private void Update()
        {
            if(_networkManager.IsServer)
                ServerUpdate();
            ClientUpdate();
        }

        public abstract void ClientUpdate();

        public abstract void ServerUpdate();
    }
}