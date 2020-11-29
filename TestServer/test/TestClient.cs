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

            BuildMessage buildMessage = new BuildMessage("test", new Vector(),new Vector());
            sendMessage(buildMessage);
            
            ConnectMessage connectMessage = new ConnectMessage();
            
            sendMessage(new DisconnectMessage());
            
            Thread.Sleep(2000);
            //client.Close();
        }

        private void sendMessage(NetworkMessage networkMessage)
        {
            RawDataMessage rMessage = NetworkConverter.Serialize(networkMessage);
            byte[] header = BitConverter.GetBytes(rMessage.data.Length);
            client.GetStream().Write(header, 0, header.Length);
            client.GetStream().Write(rMessage.data, 0, rMessage.data.Length);
        }
    }
}