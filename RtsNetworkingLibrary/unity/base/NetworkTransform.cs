using System;
using RtsNetworkingLibrary.networking.manager;
using UnityEngine;

namespace RtsNetworkingLibrary.unity.@base
{
    public class NetworkTransform : MonoBehaviour
    {
        
        public Vector3 _lastPos;
        public Vector3 _lastRot;
        public Vector3 _nextPos;
        public Vector3 _nextRot;

        private Vector3 _lerpPos;
        
        private float _deltaInterpolation = 0;

        [HideInInspector]
        public bool transformChanged = false;

        protected void Start()
        {
            _lastPos = transform.position;
            _lerpPos = _lastPos;
            _nextPos = _lastPos;
            _lastRot = transform.rotation.eulerAngles;
            _nextRot = _lastRot;
        }

        public void UpdateNetworkTransform(bool isLocalPlayer)
        {
            if (isLocalPlayer)
            {
                if (!Compare(transform.position, _lastPos) || !Compare(transform.rotation.eulerAngles, _lastRot))
                {
                    _lastPos = transform.position;
                    _lastRot = transform.rotation.eulerAngles;
                    transformChanged = true;
                }
            }else if (!Compare(transform.position, _nextPos) || !Compare(transform.rotation.eulerAngles, _nextRot))
            {
                _deltaInterpolation += Time.deltaTime * NetworkManager.Instance.ServerSettings.sendUpdateThresholdPerSecond;
                transform.position = Vector3.Lerp(_lerpPos, _nextPos, _deltaInterpolation);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_nextRot), Time.deltaTime * NetworkManager.Instance.ServerSettings.sendUpdateThresholdPerSecond);
            }   
        }


        private bool Compare(Vector3 one, Vector3 two)
        {
            float precision = .05f;
            return !(Math.Abs(one.x - two.x) > precision || Math.Abs(one.y - two.y) > precision || Math.Abs(one.z - two.z) > precision);
        }
        
        
        public void SetNextTransform(Vector3 nextPos, Vector3 nextRot)
        {
            this._nextPos = nextPos;
            this._nextRot = nextRot;
            this._lerpPos = transform.position;
            _deltaInterpolation = 0;
        }
    }
}