using System;
using System.Reflection;
using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.unity.attributes;
using UnityEngine;

namespace RtsNetworkingLibrary.unity.@base
{
    public abstract class NetworkMonoBehaviour : MonoBehaviour
    {

        private NetworkManager _networkManager;
        public int clientId;
        public ulong entityId;

        public bool IsLocalPlayer { get; private set; } = true;
        
        // Todo get a list of syncvars and check if they have changed
        
        private void Start()
        {
            this._networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            if (_networkManager == null)
            {
                throw new Exception(
                    "Network Manager not found! You should have a GameObject called \"NetworkManager\" in your project, that has the <NetworkManager> script on it!");
            }

            IsLocalPlayer = (_networkManager.ClientId == clientId);
            Debug.Log("Is local player: " + IsLocalPlayer);
            FieldInfo[] objectFields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < objectFields.Length; i++)
            {
                SyncVar attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(SyncVar)) as SyncVar;
                if (attribute != null)
                    Debug.Log(objectFields[i].Name); // The name of the flagged variable.
            }
        }

        private void Update()
        {
            if (_networkManager.ClientId != clientId)
                return;
            if(_networkManager.IsServer)
                ServerUpdate();
            ClientUpdate();
        }

        public abstract void ClientUpdate();

        public abstract void ServerUpdate();
    }
}