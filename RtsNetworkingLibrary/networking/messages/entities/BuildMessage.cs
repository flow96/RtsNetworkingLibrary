using System;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.utils;


namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class BuildMessage : NetworkMessage
    {
        public readonly string prefabName;
        public ulong entityId;
        public readonly Vector position;
        public readonly Vector rotation;
        
        public BuildMessage(string prefabName, Vector position, Vector rotation, ulong entityId = 0)
        {
            this.prefabName = prefabName;
            this.entityId = entityId;
            this.position = position;
            this.rotation = rotation;
        }
    }
}