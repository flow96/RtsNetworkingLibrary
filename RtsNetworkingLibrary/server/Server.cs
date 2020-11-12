using System;
using System.Net;
using System.Net.Sockets;
using RtsNetworkingLibrary.server.utils;

namespace RtsNetworkingLibrary.server
{
    public class Server
    {
        private uint clientCounter = 0;
        
        private TcpListener _server;
        private UdpClient _udpClient;

        // Clients
        private ClientHandler[] clients;
        private ServerSettings _serverSettings;
        
        public bool ServerRunning { private set; get; } = false;

        
        public void StartServer(ServerSettings settings)
        {
            if (!ServerRunning)
            {
                this._serverSettings = settings;
                _server = new TcpListener(IPAddress.Any, settings.port);
                clients = new ClientHandler[settings.maxPlayers];
                ServerRunning = true;
                _server.Start(10);
                _server.BeginAcceptTcpClient(AcceptTcpClients, null);
                //Debug.Log("Test");
            }
        }

        public void StopServer()
        {
            foreach (ClientHandler client in clients)
            {
                client._socket.Close();
                client._socket.Dispose();
            }
            _server.Server.Shutdown(SocketShutdown.Both);
            _server.Stop();
            _server.Server.Dispose();
            ServerRunning = false;
            clientCounter = 0;
        }

        private void AcceptTcpClients(IAsyncResult ar)
        {
            TcpClient client = _server.EndAcceptTcpClient(ar);
            clients[clientCounter++] = new ClientHandler(client, this);
            if(clientCounter < _serverSettings.maxPlayers)
                _server.BeginAcceptTcpClient(AcceptTcpClients, null);
            //Debug.Log(" >> New client connected");
            byte[] data = BitConverter.GetBytes(clientCounter);
            client.GetStream().Write(data, 0, data.Length);
            client.Close();
        }

    }
}