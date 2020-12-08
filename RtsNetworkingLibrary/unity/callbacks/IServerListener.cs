using RtsNetworkingLibrary.networking;

namespace RtsNetworkingLibrary.unity.callbacks
{
    public interface IServerListener
    {
        void OnClientConnected(PlayerInfo playerInfo);
        void OnClientDisconnected(PlayerInfo playerInfo);
        void OnServerStarted();
        void OnServerStopped();
    }
}