

using System;

namespace RtsNetworkingLibrary.networking.messages.@base
{
    [Serializable]
    public class RawDataMessage
    {
        // Readonly byte-array - the network data (should not be changed - thus readonly)
        public readonly byte[] data;

        public RawDataMessage(byte[] data)
        {
            this.data = data;
        }
    }
}