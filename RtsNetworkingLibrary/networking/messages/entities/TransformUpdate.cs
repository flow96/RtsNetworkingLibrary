using System;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.utils;

namespace RtsNetworkingLibrary.networking.messages.entities
{
    [Serializable]
    public class TransformUpdate : NetworkMessage
    {
        public Vector position;
        public Vector rotation;
        public ulong entityId;

        public TransformUpdate(Vector position, Vector rotation, ulong entityId)
        {
            this.position = position;
            this.rotation = rotation;
            this.entityId = entityId;
        }
    }
}