using RtsNetworkingLibrary.networking;

namespace RtsNetworkingLibrary.unity.callbacks
{
    public interface IClientListener
    {
        void OnConnected();
        void OnDisconnected();
        void OtherPlayerConnected(PlayerInfo playerInfo);
        void OtherPlayerDisconnected(PlayerInfo playerInfo);
    }
}