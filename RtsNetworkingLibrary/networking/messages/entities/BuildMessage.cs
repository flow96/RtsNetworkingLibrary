using System;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.utils;


namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class BuildMessage : NetworkMessage
    {
        public readonly string prefabName;
        public readonly uint id;
        public readonly Vector position;
        public readonly Vector rotation;
        
        public BuildMessage(string username, int userId, string prefabName, Vector position, Vector rotation, uint id = 0) : base(username, userId)
        {
            this.prefabName = prefabName;
            this.id = id;
            this.position = position;
            this.rotation = rotation;
        }
    }
}