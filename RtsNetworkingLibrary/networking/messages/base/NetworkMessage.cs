using System;

namespace RtsNetworkingLibrary.networking.messages.@base
{
    [Serializable]
    public abstract class NetworkMessage
    {
        public PlayerInfo playerInfo = new PlayerInfo(-1, "");
    }
}