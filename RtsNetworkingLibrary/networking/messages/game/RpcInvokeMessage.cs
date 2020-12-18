using System;
using RtsNetworkingLibrary.networking.messages.@base;

namespace RtsNetworkingLibrary.networking.messages.game
{
    [Serializable]
    public class RpcInvokeMessage : NetworkMessage
    {
        public readonly string methodName;
        public readonly ulong entityId;
        public readonly RpcTarget target;
        public readonly object[] arguments;
        
        public enum RpcTarget
        {
            All,
            ExcludeMe
        }
        
        public RpcInvokeMessage(string methodName, ulong entityId, RpcTarget target, params object[] arguments)
        {
            this.methodName = methodName;
            this.entityId = entityId;
            this.target = target;
            this.arguments = arguments;
        }
    }
}