using System;

namespace RtsNetworkingLibrary.networking.messages.@base
{
    [Serializable]
    public abstract class NetworkMessage
    {
        public readonly int userId;
        public readonly string username;

        protected NetworkMessage(string username, int userId)
        {
            this.userId = userId;
            this.username = username;
        }
    }
}