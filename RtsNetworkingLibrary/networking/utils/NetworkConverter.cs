﻿using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using RtsNetworkingLibrary.networking.messages.@base;

namespace RtsNetworkingLibrary.networking.utils
{
    public static class NetworkConverter
    {
        /**
         * Converts a NetworkMessage into a RawDataMessage (byte-array)
         * so the message can be sent over the network
         */
        public static byte[] Serialize(NetworkMessage networkMessage) {
            using (var memoryStream = new MemoryStream()) {
                (new BinaryFormatter()).Serialize(memoryStream, networkMessage);
                return memoryStream.ToArray();
            }
        }

        /**
         * Converts a received RawDataMessage into a basic NetworkMessage
         */
        public static NetworkMessage Deserialize(byte[] data) {
            using (var memoryStream = new MemoryStream(data))
                return (NetworkMessage)(new BinaryFormatter()).Deserialize(memoryStream);
        }
    }
}