using System;
using UnityEngine;

namespace RtsNetworkingLibrary.server.utils
{
    [Serializable]
    public class ServerSettings : MonoBehaviour
    {
        [Header("Server settings")]
        public string serverIp = "192.168.178.37";
        public int port = 4045;
        [Header("Lobby settings")]
        public int maxPlayers = 2;
        [Header("Debug settings")]
        public int maxHandledMessagesPerFrame = 80;
        // Messages will be sent at max. x times a second
        public int sendUpdateThresholdPerSecond = 4;
        public bool debugLogging = true;
        // The length of the header of each message
        public readonly byte headerBufferByteSize = 4;
    }
}