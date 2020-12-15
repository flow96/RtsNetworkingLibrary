using System;
using System.Collections.Generic;
using System.Reflection;
using RtsNetworkingLibrary.networking.manager;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.unity.attributes;
using RtsNetworkingLibrary.utils;
using UnityEngine;

namespace RtsNetworkingLibrary.unity.@base
{
    public abstract class NetworkMonoBehaviour : MonoBehaviour
    {

        public string prefabName = "";
        
        public int clientId;
        public ulong entityId;
        public bool syncTransform = true;

        private Vector3 _lastPos;
        private Vector3 _lastRot;
        private Vector3 _nextPos;
        private Vector3 _nextRot;

        public bool IsLocalPlayer { get; private set; } = true;
        
        private readonly Dictionary<object, object> _syncVars = new Dictionary<object, object>();
        private float _delay = 0;
        private float _fixedDealy = 0;
        private float _deltaInterpolation = 0;


        private void Awake()
        {
            _nextPos = transform.position;
            _nextRot = transform.rotation.eulerAngles;
            OnAwake();
        }

        private void Start()
        {
            if (NetworkManager.Instance == null)
            {
                throw new Exception(
                    "Network Manager not found! You should have a GameObject called \"NetworkManager\" in your project, that has the <NetworkManager> script on it!");
            }

            if (entityId == 0)
            {
                throw new Exception("A NetworkMonoBehaviour Object must be instantiated over the network!");
            }

            _fixedDealy = NetworkManager.Instance.ServerSettings.sendUpdateThresholdPerSecond / 60f;
            _lastPos = transform.position;
            _lastRot = transform.rotation.eulerAngles;
            IsLocalPlayer = (NetworkManager.Instance.ClientId == clientId);
            
            FieldInfo[] objectFields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < objectFields.Length; i++)
            {
                SyncVar attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(SyncVar)) as SyncVar;
                if (attribute != null)
                    _syncVars.Add(objectFields[i].Name, objectFields[i].GetValue(this));
            }
            OnStart();
        }

        private void Update()
        {
            if(NetworkManager.Instance.IsServer)
                ServerUpdate();
            ClientUpdate(IsLocalPlayer);
            if (IsLocalPlayer)
            {
                _delay += Time.deltaTime;
                if (_delay >= _fixedDealy)
                {
                    _delay = 0;
                    if(syncTransform)
                        UpdateTransform();
                    UpdateSyncVars();
                }
            }
            else
            {
                if (!Compare(transform.position, _nextPos) || !Compare(transform.rotation.eulerAngles, _nextRot))
                {                    
                    _deltaInterpolation += Time.deltaTime * NetworkManager.Instance.ServerSettings.sendUpdateThresholdPerSecond;
                    transform.position = Vector3.Lerp(transform.position, _nextPos, _deltaInterpolation);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_nextRot), Time.deltaTime * NetworkManager.Instance.ServerSettings.sendUpdateThresholdPerSecond);
                }
            }
        }

        public void SetNextTransform(Vector3 nextPos, Vector3 nextRot)
        {
            this._nextPos = nextPos;
            this._nextRot = nextRot;
            _deltaInterpolation = 0;
        }

        private void UpdateTransform()
        {
            if (!Compare(transform.position, _lastPos)|| !Compare(transform.rotation.eulerAngles, _lastRot))
            {
                _lastPos = transform.position;
                _lastRot = transform.rotation.eulerAngles;

                NetworkManager.Instance.EnqueOutboundUpdateMessage(new TransformUpdateMessage(new Vector(_lastPos.x, _lastPos.y, _lastPos.z), 
                    new Vector(_lastRot.x, _lastRot.y, _lastRot.z), entityId));
                /*
                NetworkManager.Instance.TcpSendToServer(new TransformUpdateMessage(new Vector(_lastPos.x, _lastPos.y, _lastPos.z), 
                    new Vector(_lastRot.x, _lastRot.y, _lastRot.z), entityId));
                    */
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
                    List<UpdateSyncVarMessage.SyncVarData> changes = new List<UpdateSyncVarMessage.SyncVarData>();
                    if (_syncVars.ContainsKey(objectFields[i].Name) &&
                                              !_syncVars[objectFields[i].Name].Equals(objectFields[i].GetValue(this)))
                    {
                        _syncVars[objectFields[i].Name] = objectFields[i].GetValue(this);
                        changes.Add(new UpdateSyncVarMessage.SyncVarData(objectFields[i].Name, objectFields[i].GetValue(this)));
                    }
                    if (changes.Count > 0)
                    {
                        UpdateSyncVarMessage msg = new UpdateSyncVarMessage(changes.ToArray(), this.entityId);
                        NetworkManager.Instance.TcpSendToServer(msg);
                    }
                }
            }
        }

        public void UpdateSyncVars(UpdateSyncVarMessage.SyncVarData[] data)
        {
            FieldInfo[] objectFields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < objectFields.Length; i++)
            {
                foreach (UpdateSyncVarMessage.SyncVarData syncVarData in data)
                {
                    if (objectFields[i].Name == syncVarData.name)
                    {
                        objectFields[i].SetValue(this, syncVarData.value);
                        break;
                    }
                }
            }
        }
        
        private bool Compare(Vector3 one, Vector3 two)
        {
            float precision = .05f;
            return !(Math.Abs(one.x - two.x) > precision || Math.Abs(one.y - two.y) > precision || Math.Abs(one.z - two.z) > precision);
        }

        public virtual void OnAwake(){}

        public virtual void OnStart(){}

        public abstract void ClientUpdate(bool isLocalPlayer);

        public abstract void ServerUpdate();
    }
}