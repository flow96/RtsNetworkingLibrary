using UnityEngine;

namespace RtsNetworkingLibrary.server.utils
{
    public class ServerSettings : MonoBehaviour
    {
        public int port = 4045;
        public int maxPlayers = 2;
        public int maxHandledMessagesPerFrame = 80;
        public int sendUpdateThresholdPerSecond = 10;    // Messages will be sent at max. 10 times a second
        public string serverIp = "192.168.178.37";
        
        public readonly byte headerBufferByteSize = 4; // The length of the header of each message (defines the length of the actual message)
        
    }
}