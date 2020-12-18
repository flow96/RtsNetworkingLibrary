using System;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.utils;

namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class TransformUpdateMessage : NetworkMessage
    {
        public readonly Vector position;
        public readonly Vector rotation;
        public readonly ulong entityId;
        public readonly int subTransformIndex;

        public TransformUpdateMessage(Vector position, Vector rotation, ulong entityId, int subTransformIndex = -1)
        {
            this.position = position;
            this.rotation = rotation;
            this.entityId = entityId;
            this.subTransformIndex = subTransformIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj is TransformUpdateMessage)
            {
                TransformUpdateMessage other = (TransformUpdateMessage) obj;
                return other.entityId == this.entityId;
            }
            return false;
        }
    }
}