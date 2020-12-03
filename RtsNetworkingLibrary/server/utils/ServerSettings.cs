using UnityEngine;

namespace RtsNetworkingLibrary.server.utils
{
    public class ServerSettings : MonoBehaviour
    {
        public int port = 4045;
        public int maxPlayers = 2;
        public int maxHandledMessagesPerFrame = 80;
        
        
        public readonly byte headerBufferByteSize = 4; // The length of the header of each message (defines the length of the actual message)
        
    }
}