using System;
using RtsNetworkingLibrary.networking.messages.@base;

namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class DestroyMessage : NetworkMessage
    {
        public readonly ulong entityId;
        
        public DestroyMessage(ulong entityId)
        {
            this.entityId = entityId;
        }
    }
}