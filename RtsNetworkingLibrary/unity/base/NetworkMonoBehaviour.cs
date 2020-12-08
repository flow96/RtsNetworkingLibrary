using System;
using System.Collections.Generic;
using System.Reflection;
using RtsNetworkingLibrary.networking;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.unity.attributes;
using RtsNetworkingLibrary.utils;
using UnityEngine;
using Logger = RtsNetworkingLibrary.utils.Logger;

namespace RtsNetworkingLibrary.unity.@base
{
    public abstract class NetworkMonoBehaviour : MonoBehaviour
    {

        private NetworkManager _networkManager;
        public int clientId;
        public ulong entityId;
        public bool syncTransform = true;

        private Transform _lastTransform;
        private Transform _nextTransform;    // Will be set by the server, to move an enemy entity to a desired position

        public bool IsLocalPlayer { get; private set; } = true;
        
        // Todo get a list of syncvars and check if they have changed
        private readonly Dictionary<object, object> _syncVars = new Dictionary<object, object>();
        private int _delay = 0;
        
        private void Start()
        {
            this._networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            if (_networkManager == null)
            {
                throw new Exception(
                    "Network Manager not found! You should have a GameObject called \"NetworkManager\" in your project, that has the <NetworkManager> script on it!");
            }

            if (entityId == 0)
            {
                throw new Exception("A NetworkMonoBehaviour Object must be instantiated over the network!");
            }
            _lastTransform = transform;
            IsLocalPlayer = (_networkManager.ClientId == clientId);
            Debug.Log("Is local player: " + IsLocalPlayer);
            FieldInfo[] objectFields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < objectFields.Length; i++)
            {
                SyncVar attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(SyncVar)) as SyncVar;
                if (attribute != null)
                    _syncVars.Add(objectFields[i].Name, objectFields[i].GetRawConstantValue());
            }
        }

        private void Update()
        {
            if(_networkManager.IsServer)
                ServerUpdate();
            ClientUpdate(IsLocalPlayer);
            if (IsLocalPlayer)
            {
                --_delay;
                if (_delay <= 0)
                {
                    _delay = 60 / _networkManager.ServerSettings.sendUpdateThreshold;
                    if(syncTransform)
                        UpdateTransform();
                    UpdateSyncVars();
                }
            }
        }

        private void UpdateTransform()
        {
            if (transform.position != _lastTransform.position || transform.rotation != _lastTransform.rotation)
            {
                _lastTransform = transform;
                var position = _lastTransform.position;
                var rotation = _lastTransform.eulerAngles;
                _networkManager.TcpSendToServer(new TransformUpdate(new Vector(position.x, position.y, position.z), 
                    new Vector(rotation.x, rotation.y, rotation.z), entityId));
            }
        }

        private void UpdateSyncVars()
        {
            FieldInfo[] objectFields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < objectFields.Length; i++)
            {
                SyncVar attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(SyncVar)) as SyncVar;
                if (attribute != null)
                {
                    if (_syncVars.ContainsKey(objectFields[i].Name) &&
                                              _syncVars[objectFields[i]] != objectFields[i].GetRawConstantValue())
                    {
                        Debug.Log("Syncvar has changed: " + objectFields[i].Name + "from: " + _syncVars[objectFields[i]] + " to: " + objectFields[i].GetRawConstantValue());
                        _syncVars[objectFields[i]] = objectFields[i].GetRawConstantValue();
                    }
                }
            }
        }

        public abstract void ClientUpdate(bool isLocalPlayer);

        public abstract void ServerUpdate();
    }
}