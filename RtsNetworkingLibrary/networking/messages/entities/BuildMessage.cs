using System;
using RtsNetworkingLibrary.networking.messages.@base;


namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class BuildMessage : NetworkMessage
    {
        public readonly string prefabName;
        public readonly uint id;
        
        public BuildMessage(string username, int userId, string prefabName, uint id = 0) : base(username, userId)
        {
            this.prefabName = prefabName;
            this.id = id;
        }
    }
}