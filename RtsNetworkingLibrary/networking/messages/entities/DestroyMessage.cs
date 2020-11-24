using System;
using RtsNetworkingLibrary.networking.messages.@base;

namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class DestroyMessage : NetworkMessage
    {
        public readonly uint id;
        
        public DestroyMessage(string username, int userId, uint id) : base(username, userId)
        {
            this.id = id;
        }
    }
}