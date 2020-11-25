using System;
using RtsNetworkingLibrary.networking.messages.@base;

namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class DestroyMessage : NetworkMessage
    {
        public readonly int id;
        
        public DestroyMessage(int buildingId)
        {
            this.id = buildingId;
        }
    }
}