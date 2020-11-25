using System;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.utils;


namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class BuildMessage : NetworkMessage
    {
        public readonly string prefabName;
        public readonly int id;
        public readonly Vector position;
        public readonly Vector rotation;
        
        public BuildMessage(string prefabName, Vector position, Vector rotation, int id = -1)
        {
            this.prefabName = prefabName;
            this.id = id;
            this.position = position;
            this.rotation = rotation;
        }
    }
}