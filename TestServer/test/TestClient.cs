using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.messages.entities;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.utils;
using Logger = RtsNetworkingLibrary.utils.Logger;

namespace TestServer.test
{
    
    
    public class TestClient
    {
        private readonly Logger _logger;
        
        private static int port = 4045;

        private TcpClient client;
        private int clientId = 0;
        
        static void Main(string[] args)
        {
            Logger.LoggerType = LoggerType.DEDICATED;
            //new TestServer();
            new TestClient();
            
            //Thread.Sleep(2000);
            Console.ReadKey();
            
        }

        TestClient()
        {
            _logger = new Logger(GetType().Name);
            IPAddress ip = Array.FindLast(
                Dns.GetHostEntry(string.Empty).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
            _logger.Debug("Ip Endpoint");
            _logger.Debug(ipEndPoint);
            client = new TcpClient();
            client.Connect(ipEndPoint);
            _logger.Debug("Connected: " + client.Connected);

            sendMessage(new ConnectMessage(Environment.UserName));
            ConnectMessage msg = (ConnectMessage)ReceiveSingleMessage(client);
            this.clientId = msg.userId;
            
            BuildMessage buildMessage = new BuildMessage("test", new Vector(), new Vector());
            sendMessage(buildMessage);
            
            Thread.Sleep(2000);
            //client.Close();
        }
        
        
        private NetworkMessage ReceiveSingleMessage(TcpClient client)
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

        private void sendMessage(NetworkMessage networkMessage)
        {
            networkMessage.userId = this.clientId;
            byte[] data = NetworkConverter.Serialize(networkMessage);
            byte[] header = BitConverter.GetBytes(data.Length);
            client.GetStream().Write(header, 0, header.Length);
            client.GetStream().Write(data, 0, data.Length);
        }
    }
}