using System;
using RtsNetworkingLibrary.networking.messages.@base;

namespace RtsNetworkingLibrary.networking.messages.game
{
    [Serializable]
    public class StartGameMessage : NetworkMessage
    {
        public readonly string sceneName;

        public StartGameMessage(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }
}