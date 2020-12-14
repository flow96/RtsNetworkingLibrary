using UnityEngine;

namespace RtsNetworkingLibrary.networking.manager
{
    public interface IInstantiateListener
    {
        void OnGameObjectCreated(GameObject gameObject);
    }
}