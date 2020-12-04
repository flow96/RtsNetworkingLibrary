using System;
using System.Net.Sockets;
using RtsNetworkingLibrary.networking.messages.@base;

namespace RtsNetworkingLibrary.networking.utils
{
    public static class NetworkHelper
    {
        public static NetworkMessage ReceiveSingleMessage(TcpClient client)
        {
            byte[] dataBuffer = new byte[4];
            int headerLength = 0, msgDataLength = 0, msgReadLength = 0;
            do
            {
                headerLength += client.GetStream().Read(dataBuffer, 0, 4 - headerLength);    
            } while (headerLength < 4);
            msgDataLength = BitConverter.ToInt32(dataBuffer, 0);
            dataBuffer = new byte[msgDataLength];
            do
            {
                msgReadLength += client.GetStream().Read(dataBuffer, 0, msgDataLength - msgReadLength);
            } while (msgReadLength < msgDataLength);
            return (NetworkConverter.Deserialize(dataBuffer));
        }
        
        
        public static void SendSingleMessage(TcpClient client, NetworkMessage message, int clientId)
        {
            message.userId = clientId;
            byte[] data = NetworkConverter.Serialize(message);
            byte[] header = BitConverter.GetBytes(data.Length);
            client.GetStream().Write(header, 0, header.Length);
            client.GetStream().Write(data, 0, data.Length);
        }

    }
}