using System;
using System.Collections.Generic;
using System.Reflection;
using RtsNetworkingLibrary.networking.manager;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.networking.messages.game;
using RtsNetworkingLibrary.unity.attributes;
using RtsNetworkingLibrary.utils;
using UnityEngine;

namespace RtsNetworkingLibrary.unity.@base
{
    public abstract class NetworkMonoBehaviour : NetworkTransform
    {

        [Header("Networking")]
        public string prefabName = "";
        
        public int clientId;
        public ulong entityId;
        public bool syncTransform = true;
        public bool IsSelected { get; set; } = false;

        private readonly List<NetworkTransform> _subTransforms = new List<NetworkTransform>();

        public bool IsLocalPlayer { get; private set; } = true;
        
        private readonly Dictionary<object, object> _syncVars = new Dictionary<object, object>();
        private float _delay = 0;
        private float _fixedDealy = 0;


        private void Awake()
        {
            _nextPos = transform.position;
            _nextRot = transform.rotation.eulerAngles;
            FindSubNetworkTransforms(transform);
            Debug.Log("Found: " + _subTransforms.Count + " childs");
            OnAwake();
        }

        private void FindSubNetworkTransforms(Transform root)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                NetworkTransform t = child.GetComponent<NetworkTransform>();
                if(t != null)
                    _subTransforms.Add(t);
                FindSubNetworkTransforms(child);
            }
        }

        private void Start()
        {
            base.Start();
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
            UpdateNetworkTransform(IsLocalPlayer);
            _subTransforms.ForEach(networkTransform => networkTransform.UpdateNetworkTransform(IsLocalPlayer));
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
        }

        

        private void UpdateTransform()
        {
            if (transformChanged)
            {
                transformChanged = false;
                NetworkManager.Instance.EnqueOutboundUpdateMessage(new TransformUpdateMessage(new Vector(_lastPos.x, _lastPos.y, _lastPos.z), 
                    new Vector(_lastRot.x, _lastRot.y, _lastRot.z), entityId));
            }
            
            for (int i = 0; i < _subTransforms.Count; i++)
            {
                if (_subTransforms[i].transformChanged)
                {
                    _subTransforms[i].transformChanged = false;
                    NetworkManager.Instance.EnqueOutboundUpdateMessage(new TransformUpdateMessage(new Vector(_subTransforms[i]._lastPos.x, _subTransforms[i]._lastPos.y, _subTransforms[i]._lastPos.z), 
                        new Vector(_subTransforms[i]._lastRot.x, _subTransforms[i]._lastRot.y, _subTransforms[i]._lastRot.z), entityId, i));
                }   
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

        public void UpdateChildTransform(int index, Vector3 nextPos, Vector3 nextRot)
        {
            if (_subTransforms.Count > index)
            {
                _subTransforms[index].SetNextTransform(nextPos, nextRot);
            }
        }
        
        private bool Compare(Vector3 one, Vector3 two)
        {
            float precision = .05f;
            return !(Math.Abs(one.x - two.x) > precision || Math.Abs(one.y - two.y) > precision || Math.Abs(one.z - two.z) > precision);
        }

        public void Rpc(string methodName, RpcInvokeMessage.RpcTarget target, params object[] arguments)
        {
            NetworkManager.Instance.TcpSendToServer(new RpcInvokeMessage(methodName, entityId, target, arguments));
        }

        public void HandleExternalRpcInvoke(string name, params object[] arguments)
        {
            var method = this.GetType().GetMethod(name);
            if (method != null)
            {
                method.Invoke(this, arguments);   
            }
        }

        public virtual void OnAwake(){}

        public virtual void OnStart(){}

        public abstract void ClientUpdate(bool isLocalPlayer);

        public abstract void ServerUpdate();
    }
}