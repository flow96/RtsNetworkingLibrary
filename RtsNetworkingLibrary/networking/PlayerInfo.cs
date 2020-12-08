using System;

namespace RtsNetworkingLibrary.networking
{
    [Serializable]
    public class PlayerInfo
    {
        public int userId;
        public string username;

        public PlayerInfo(int userId, string username)
        {
            this.userId = userId;
            this.username = username;
        }
    }
}