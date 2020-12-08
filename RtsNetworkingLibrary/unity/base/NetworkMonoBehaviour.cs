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

        private Vector3 _lastPos;
        private Vector3 _lastRot;
        private Vector3 _nextPos;
        private Vector3 _nextRot;

        public bool IsLocalPlayer { get; private set; } = true;
        
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
            _lastPos = transform.position;
            _lastRot = transform.rotation.eulerAngles;
            IsLocalPlayer = (_networkManager.ClientId == clientId);
            Debug.Log("Is local player: " + IsLocalPlayer);
            
            FieldInfo[] objectFields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < objectFields.Length; i++)
            {
                SyncVar attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(SyncVar)) as SyncVar;
                if (attribute != null)
                    _syncVars.Add(objectFields[i].Name, objectFields[i].GetValue(this));
            }
            Debug.Log("Hi");
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
                    _delay = 60 / _networkManager.ServerSettings.sendUpdateThresholdPerSecond;
                    if(syncTransform)
                        UpdateTransform();
                    UpdateSyncVars();
                }
            }
            else
            {
                if (!Compare(transform.position, _nextPos) || !Compare(transform.rotation.eulerAngles, _nextRot))
                {
                    transform.position = Vector3.Lerp(transform.position, _nextPos,
                        Time.deltaTime * _networkManager.ServerSettings.sendUpdateThresholdPerSecond);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_nextRot), Time.deltaTime * _networkManager.ServerSettings.sendUpdateThresholdPerSecond);
                }
            }
        }

        public void SetNextTransform(Vector3 nextPos, Vector3 nextRot)
        {
            this._nextPos = nextPos;
            this._nextRot = nextRot;
        }

        private void UpdateTransform()
        {
            if (!Compare(transform.position, _lastPos)|| !Compare(transform.rotation.eulerAngles, _lastRot))
            {
                _lastPos = transform.position;
                _lastRot = transform.rotation.eulerAngles;
                _networkManager.TcpSendToServer(new TransformUpdate(new Vector(_lastPos.x, _lastPos.y, _lastPos.z), 
                    new Vector(_lastRot.x, _lastRot.y, _lastRot.z), entityId));
            }
        }

        private bool Compare(Vector3 one, Vector3 two)
        {
            float precision = .05f;
            return !(Math.Abs(one.x - two.x) > precision || Math.Abs(one.y - two.y) > precision || Math.Abs(one.z - two.z) > precision);
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
                                              !_syncVars[objectFields[i].Name].Equals(objectFields[i].GetValue(this)))
                    {
                        Debug.Log("Syncvar has changed: '" + objectFields[i].Name + "' => from: " + _syncVars[objectFields[i].Name] + " to: " + objectFields[i].GetValue(this));
                        _syncVars[objectFields[i].Name] = objectFields[i].GetValue(this);
                        Debug.Log(_syncVars[objectFields[i].Name].Equals(objectFields[i].GetValue(this)));
                    }
                }
            }
        }

        public abstract void ClientUpdate(bool isLocalPlayer);

        public abstract void ServerUpdate();
    }
}