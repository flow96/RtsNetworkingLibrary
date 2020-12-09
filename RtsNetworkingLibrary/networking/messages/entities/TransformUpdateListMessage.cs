using System;
using RtsNetworkingLibrary.networking.messages.@base;

namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class TransformUpdateListMessage : NetworkMessage
    {
        public readonly TransformUpdateMessage[] updates;

        public TransformUpdateListMessage(TransformUpdateMessage[] updates)
        {
            this.updates = updates;
        }
    }
}