using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RtsNetworkingLibrary.networking.messages.@base;
using RtsNetworkingLibrary.networking.messages.connection;
using RtsNetworkingLibrary.networking.utils;
using RtsNetworkingLibrary.server;
using RtsNetworkingLibrary.utils;

namespace TestServer.test
{
    
    
    public class TestClient
    {
        private readonly Logger _logger;
        
        private static int port = 4045;
        
        static void Main(string[] args)
        {
            Logger.LoggerType = LoggerType.DEDICATED;
            new TestServer();
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
            TcpClient client = new TcpClient();
            client.Connect(ipEndPoint);
            _logger.Debug("Connected: " + client.Connected);

            ConnectMessage connectMessage = new ConnectMessage(System.Environment.UserName.ToString(), -1);
            RawDataMessage rawMessage = NetworkConverter.Serialize(connectMessage);
            byte[] headerBuffer = BitConverter.GetBytes(rawMessage.data.Length);

            for (int i = 0; i < 1; i++)
            {
                client.GetStream().Write(headerBuffer,0,headerBuffer.Length);
                client.GetStream().Write(rawMessage.data, 0,rawMessage.data.Length);
            
                _logger.Debug(" >> Client message sent " + i);
            }
            
            client.Close();
        }
    }
}