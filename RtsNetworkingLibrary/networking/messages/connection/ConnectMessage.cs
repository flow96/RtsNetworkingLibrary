using System;
using RtsNetworkingLibrary.networking.messages.@base;

namespace RtsNetworkingLibrary.networking.messages.connection
{
    [Serializable]
    public class ConnectMessage : NetworkMessage
    {
        public ConnectMessage(string username, int userId) : base(username, userId)
        {
        }
    }
}