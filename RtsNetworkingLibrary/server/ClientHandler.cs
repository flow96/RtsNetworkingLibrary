using System.Net.Sockets;

namespace RtsNetworkingLibrary.server
{
    public class ClientHandler
    {

        public readonly TcpClient _socket;
        public readonly Server _server;
        private byte[] _readBuffer;
        
        public ClientHandler(TcpClient socket, Server server)
        {
            this._socket = socket;
            this._server = server;
        }
    }
}