using System;
using System.Net;
using System.Net.Sockets;

namespace TestServer.test
{
    
    
    public class TestClient
    {
        private static int port = 4045;
        
        static void Main(string[] args)
        {
            // Server server = new Server(port);
            // server.StartServer();
            // Thread.Sleep(5000);
            //new TestServer();
            new TestClient();
        }

        TestClient()
        {
            IPAddress ip = Array.FindLast(
                Dns.GetHostEntry(string.Empty).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
            Console.WriteLine("Ip Endpoint");
            Console.WriteLine(ipEndPoint);
            TcpClient client = new TcpClient();
            client.Connect(ipEndPoint);
            Console.WriteLine("Connected: " + client.Connected);
            
            byte[] header = new byte[4];
            int readCount = client.GetStream().Read(header, 0, header.Length);
            uint id = BitConverter.ToUInt32(header, 0);
            Console.WriteLine("Received: " + id + " ReadCount: " + readCount);
            client.Close();
        }
    }
}