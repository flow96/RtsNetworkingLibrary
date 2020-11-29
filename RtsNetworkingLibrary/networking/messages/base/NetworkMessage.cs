using System;

namespace RtsNetworkingLibrary.networking.messages.@base
{
    [Serializable]
    public abstract class NetworkMessage
    {
        public int userId;
        public string username;
    }
}